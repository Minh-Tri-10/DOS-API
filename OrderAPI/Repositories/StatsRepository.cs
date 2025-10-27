using Microsoft.EntityFrameworkCore;
using OrderAPI.DTOs;
using OrderAPI.Models;
using OrderAPI.Repositories.Interfaces;
using OrderAPI.Services.Interfaces;
namespace OrderAPI.Repositories;

public sealed class StatsRepository : IStatsRepository
{
    private readonly OrderDbContext _db;
    private readonly ICatalogProductClient _catalogProductClient;
    private readonly ICategoryClient _categoryClient;

    public StatsRepository(OrderDbContext db, ICatalogProductClient productClient, ICategoryClient categoryClient)
    {
        _db = db;
        _catalogProductClient = productClient;
        _categoryClient = categoryClient;
    }
    // --- Helpers ---
    private static DateTime NormalizeStart(DateTime start) => DateTime.SpecifyKind(start, DateTimeKind.Utc);
    private static DateTime NormalizeEnd(DateTime end)
    {
        // Use [start, end) to avoid overlaps; shift end-of-day to the next midnight
        var e = DateTime.SpecifyKind(end, DateTimeKind.Utc);
        if (e.TimeOfDay == TimeSpan.Zero) e = e.AddDays(1);
        return e;
    }

    public async Task<SummaryDto> GetSummaryAsync(DateTime start, DateTime end)
    {
        start = NormalizeStart(start);
        end = NormalizeEnd(end);

        var orders = _db.Orders.AsNoTracking()
            .Where(o => o.OrderDate >= start && o.OrderDate < end);

        var total = await orders.CountAsync();
        var paid = await orders.CountAsync(o => o.PaymentStatus == "paid");
        var cancelled = await orders.CountAsync(o => o.OrderStatus == "cancelled");
        var revenue = await orders
            .Where(o => o.PaymentStatus == "paid")
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

        var aov = paid > 0 ? revenue / paid : 0m;

        return new SummaryDto
        {
            TotalOrders = total,
            PaidOrders = paid,
            CancelledOrders = cancelled,
            Revenue = revenue,
            AvgOrderValue = aov
        };
    }

    //public async Task<List<RevenueByCategoryDto>> GetRevenueByCategoryAsync(DateTime start, DateTime end)
    //{
    //    start = NormalizeStart(start);
    //    end = NormalizeEnd(end);

    //    // Dùng navigation, tránh Join + AsQueryable().Sum gây không d?ch du?c
    //    var rows = await _db.OrderItems
    //        .AsNoTracking()
    //        .Where(oi => oi.Order.PaymentStatus == "paid"
    //                  && oi.Order.OrderDate >= start && oi.Order.OrderDate < end)
    //        .GroupBy(oi => new
    //        {
    //            oi.Product.CategoryId,
    //            CategoryName = oi.Product.Category.CategoryName
    //        })
    //        .Select(g => new RevenueByCategoryDto
    //        {
    //            CategoryId = g.Key.CategoryId ?? 0,
    //            CategoryName = g.Key.CategoryName ?? "Unknown",
    //            Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
    //        })
    //        .OrderByDescending(x => x.Revenue)
    //        .ToListAsync();

    //    return rows;
    //}

    //public async Task<List<RevenueByProductDto>> GetRevenueByProductAsync(DateTime start, DateTime end, int top)
    //{
    //    start = NormalizeStart(start);
    //    end = NormalizeEnd(end);

    //    var rows = await _db.OrderItems
    //        .AsNoTracking()
    //        .Where(oi => oi.Order.PaymentStatus == "paid"
    //                  && oi.Order.OrderDate >= start && oi.Order.OrderDate < end)
    //        .GroupBy(oi => new
    //        {
    //            oi.ProductId,
    //            ProductName = oi.Product.ProductName
    //        })
    //        .Select(g => new RevenueByProductDto
    //        {
    //            ProductId = g.Key.ProductId,
    //            ProductName = g.Key.ProductName ?? "Unknown",
    //            Quantity = g.Sum(x => x.Quantity),
    //            Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
    //        })
    //        .OrderByDescending(x => x.Revenue)
    //        .Take(top)
    //        .ToListAsync();

    //    return rows;
    //}
    public async Task<List<RevenueByProductDto>> GetRevenueByProductAsync(DateTime start, DateTime end, int top)
    {
        start = NormalizeStart(start);
        end = NormalizeEnd(end);

        var items = await _db.OrderItems
            .Where(oi => oi.Order.PaymentStatus == "paid"
                      && oi.Order.OrderDate >= start && oi.Order.OrderDate < end)
            .ToListAsync();

        var productIds = items.Select(x => x.ProductId).Distinct();
        var productTasks = productIds.Select(id => _catalogProductClient.GetProductByIdAsync(id));
        var products = await Task.WhenAll(productTasks);

        var productDict = products.Where(p => p != null)
                                  .ToDictionary(p => p!.ProductId, p => p);

        var rows = items
            .GroupBy(oi => oi.ProductId)
            .Select(g =>
            {
                var product = productDict.ContainsKey(g.Key) ? productDict[g.Key] : null;
                return new RevenueByProductDto
                {
                    ProductId = g.Key,
                    ProductName = product?.ProductName ?? "Unknown",
                    Quantity = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                };
            })
            .OrderByDescending(x => x.Revenue)
            .Take(top)
            .ToList();

        return rows;
    }

    public async Task<List<RevenueByCategoryDto>> GetRevenueByCategoryAsync(DateTime start, DateTime end)
    {
        start = NormalizeStart(start);
        end = NormalizeEnd(end);

        var items = await _db.OrderItems
            .Where(oi => oi.Order.PaymentStatus == "paid"
                      && oi.Order.OrderDate >= start && oi.Order.OrderDate < end)
            .ToListAsync();

        var productIds = items.Select(x => x.ProductId).Distinct();
        var productTasks = productIds.Select(id => _catalogProductClient.GetProductByIdAsync(id));
        var products = await Task.WhenAll(productTasks);

        var categoryIds = products.Where(p => p != null).Select(p => p!.CategoryId).Distinct();
        var categories = await _categoryClient.GetCategoriesByIdsAsync(categoryIds);

        var productDict = products.Where(p => p != null).ToDictionary(p => p!.ProductId, p => p!);
        var categoryDict = categories.ToDictionary(c => c.CategoryId, c => c);

        var rows = items
            .GroupBy(oi =>
            {
                var product = productDict.ContainsKey(oi.ProductId) ? productDict[oi.ProductId] : null;
                return product?.CategoryId ?? 0;
            })
            .Select(g =>
            {
                var category = categoryDict.ContainsKey(g.Key) ? categoryDict[g.Key] : null;
                return new RevenueByCategoryDto
                {
                    CategoryId = g.Key,
                    CategoryName = category?.CategoryName ?? "Unknown",
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                };
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        return rows;
    }

    public async Task<List<SeriesPointDto>> GetSeriesAsync(DateTime start, DateTime end, string granularity)
    {
        start = NormalizeStart(start);
        end = NormalizeEnd(end);

        // Avoid translation issues when bucketing by week or month
        // Load minimal data and group in-memory (acceptable for demo workloads)
        var baseData = await _db.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == "paid"
                     && o.OrderDate >= start && o.OrderDate < end)
            .Select(o => new { o.OrderDate, Amount = (decimal)(o.TotalAmount ?? 0m), o.OrderId })
            .ToListAsync();

        static DateTime TruncateToDay(DateTime d) => new(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc);
        static DateTime TruncateToMonth(DateTime d) => new(d.Year, d.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        static DateTime TruncateToWeekMon(DateTime d)
        {
            var dd = d.Date;
            int diff = ((int)dd.DayOfWeek + 6) % 7; // Monday=0
            var monday = dd.AddDays(-diff);
            return new DateTime(monday.Year, monday.Month, monday.Day, 0, 0, 0, DateTimeKind.Utc);
        }

        Func<DateTime, DateTime> bucketFunc = granularity?.ToLower() switch
        {
            "month" => TruncateToMonth,
            "week" => TruncateToWeekMon,
            _ => TruncateToDay
        };

        var rows = baseData
            .GroupBy(x => bucketFunc(x.OrderDate))
            .Select(g => new SeriesPointDto
            {
                Bucket = g.Key,
                Revenue = g.Sum(x => x.Amount),
                Orders = g.Select(x => x.OrderId).Distinct().Count()
            })
            .OrderBy(g => g.Bucket)
            .ToList();

        return rows;
    }
}

