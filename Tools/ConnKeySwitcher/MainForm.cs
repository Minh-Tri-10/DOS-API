using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ConnKeySwitcher;

public partial class MainForm : Form
{
    private static readonly string[] KnownKeys = new[]
    {
        "SqlServer",
        "DefaultConnection",
        "LocConnection",
        "HuyConnection",
        "TriConnection",
        "WeiConnection"
    };

    private readonly Regex _pattern = new Regex(@"GetConnectionString\(\s*""(?<key>[^""]+)""\s*\)", RegexOptions.Compiled | RegexOptions.Multiline);
    private readonly Dictionary<string, Process> _headless = new();

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        cboKey.Items.AddRange(KnownKeys.Cast<object>().ToArray());
        cboKey.SelectedIndex = Array.IndexOf(KnownKeys, "HuyConnection");

        var repo = FindRepoRoot() ?? Environment.CurrentDirectory;
        txtRoot.Text = repo;
        txtRunRoot.Text = repo;
    }

    private string? FindRepoRoot()
    {
        var dir = new DirectoryInfo(Environment.CurrentDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "DOS.sln"))) return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }

    // ========== Switch tab ==========
    private void BtnBrowse_Click(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog();
        dlg.SelectedPath = Directory.Exists(txtRoot.Text) ? txtRoot.Text : Environment.CurrentDirectory;
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            txtRoot.Text = dlg.SelectedPath;
            txtRunRoot.Text = dlg.SelectedPath;
        }
    }

    private void BtnScan_Click(object? sender, EventArgs e) => Scan();
    private void BtnApply_Click(object? sender, EventArgs e) => Apply();

    private void Scan()
    {
        grid.Rows.Clear();
        var files = EnumerateProgramFiles().ToList();
        int i = 0;
        foreach (var file in files)
        {
            string text;
            try { text = File.ReadAllText(file); }
            catch (Exception ex)
            {
                grid.Rows.Add(file, "-", 0, false, $"Read error: {ex.Message}");
                continue;
            }

            var matches = _pattern.Matches(text);
            if (matches.Count == 0) { i++; continue; }

            var keys = matches.Select(m => m.Groups["key"].Value).ToList();
            var unique = string.Join(", ", keys.Distinct());
            var target = cboKey.SelectedItem?.ToString() ?? string.Empty;
            bool willChange = keys.Any(k => !string.Equals(k, target, StringComparison.Ordinal));

            grid.Rows.Add(file, unique, matches.Count, willChange, string.Empty);
            i++;
            SetStatus($"Scanning {i}/{files.Count} files...", true, (int)(i * 100.0 / files.Count));
            Application.DoEvents();
        }
        SetStatus("Scan completed.");
    }

    private void Apply()
    {
        var selectedKey = cboKey.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(selectedKey))
        {
            MessageBox.Show(this, "Please choose a connection key.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int filesChanged = 0, locations = 0, errors = 0;
        var files = EnumerateProgramFiles().ToList();
        int idx = 0;
        foreach (var file in files)
        {
            string text;
            try { text = File.ReadAllText(file); }
            catch (Exception ex)
            {
                UpdateStatusCell(file, $"Read error: {ex.Message}");
                errors++;
                continue;
            }

            if (!_pattern.IsMatch(text)) { idx++; continue; }

            var replaced = _pattern.Replace(text, m =>
            {
                locations++;
                return $"GetConnectionString(\"{selectedKey}\")";
            });

            if (!string.Equals(text, replaced, StringComparison.Ordinal))
            {
                try
                {
                    File.WriteAllText(file, replaced, new UTF8Encoding(false));
                    filesChanged++;
                    UpdateStatusCell(file, "OK");
                }
                catch (Exception ex)
                {
                    UpdateStatusCell(file, $"Write error: {ex.Message}");
                    errors++;
                }
            }

            idx++;
            SetStatus($"Applying {idx}/{files.Count} files...", true, (int)(idx * 100.0 / files.Count));
            Application.DoEvents();
        }

        Scan();
        SetStatus("Done.");
        var msg = $"Updated {filesChanged} files, {locations} locations.";
        if (errors > 0) msg += $" Errors: {errors}.";
        MessageBox.Show(this, msg, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private IEnumerable<string> EnumerateProgramFiles()
    {
        var root = txtRoot.Text.Trim();
        if (!Directory.Exists(root)) yield break;

        foreach (var file in Directory.EnumerateFiles(root, "Program.cs", SearchOption.AllDirectories))
        {
            if (file.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) continue;
            if (file.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) continue;
            yield return file;
        }
    }

    private void UpdateStatusCell(string file, string statusTextValue)
    {
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.Cells[0]?.Value is string path && string.Equals(path, file, StringComparison.OrdinalIgnoreCase))
            {
                row.Cells["colStatus"].Value = statusTextValue;
                break;
            }
        }
    }

    private void SetStatus(string text, bool working = false, int percent = -1)
    {
        statusText.Text = text;
        statusProgress.Visible = working;
        if (percent >= 0)
        {
            statusProgress.Value = Math.Max(0, Math.Min(100, percent));
        }
    }

    // ========== Run tab ==========
    private void BtnRunBrowse_Click(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog();
        dlg.SelectedPath = Directory.Exists(txtRunRoot.Text) ? txtRunRoot.Text : txtRoot.Text;
        if (dlg.ShowDialog(this) == DialogResult.OK)
            txtRunRoot.Text = dlg.SelectedPath;
    }

    private void BtnDiscover_Click(object? sender, EventArgs e)
    {
        lstRunProjects.Items.Clear();
        var root = txtRunRoot.Text;
        if (!Directory.Exists(root)) { Log("Invalid repository root."); return; }

        foreach (var proj in Directory.EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories)
                     .Where(p => !p.Contains("\\bin\\") && !p.Contains("\\obj\\"))
                     .OrderBy(p => p))
        {
            var name = Path.GetFileNameWithoutExtension(proj);
            var rel = Path.GetRelativePath(root, proj);
            lstRunProjects.Items.Add(new ProjectItem { Name = name, Path = proj, Display = $"{name} ({rel})" }, true);
        }

        Log($"Found {lstRunProjects.Items.Count} projects.");
    }

    private void BtnRunSelected_Click(object? sender, EventArgs e)
    {
        var items = lstRunProjects.CheckedItems.Cast<ProjectItem>().ToList();
        if (items.Count == 0)
        {
            MessageBox.Show(this, "Check one or more projects to run.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        foreach (var item in items)
        {
            try
            {
                var projDir = Path.GetDirectoryName(item.Path)!;
                var launch = GetLaunchInfo(projDir);
                if (chkRunTerminal.Checked)
                {
                    var cmd = BuildTerminalCommand(item.Name, item.Path, launch.Profile, projDir);
                    Process.Start(new ProcessStartInfo(cmd.FileName, cmd.Arguments)
                    {
                        WorkingDirectory = projDir,
                        UseShellExecute = true
                    });
                    Log($"Started {item.Name} in terminal.");
                }
                else
                {
                    StartHeadless(item, launch.Profile);
                }

                if (chkRunBrowser.Checked && !string.IsNullOrEmpty(launch.Url))
                {
                    _ = OpenBrowserWithDelayAsync(launch.Url, TimeSpan.FromSeconds(5));
                    Log($"Scheduled browser launch for {launch.Url}");
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to start {item.Name}: {ex.Message}");
            }
        }
    }

    private void StartHeadless(ProjectItem item, string? profile)
    {
        if (_headless.ContainsKey(item.Path)) return;
        var args = new StringBuilder("run");
        if (!string.IsNullOrWhiteSpace(profile)) args.Append($" --launch-profile \"{profile}\"");
        args.Append(" --project \"").Append(item.Path).Append("\" --no-build");

        var psi = new ProcessStartInfo("dotnet", args.ToString())
        {
            WorkingDirectory = Path.GetDirectoryName(item.Path)!,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
        proc.OutputDataReceived += (_, a) => { if (a.Data != null) Log($"[{item.Name}] {a.Data}"); };
        proc.ErrorDataReceived += (_, a) => { if (a.Data != null) Log($"[{item.Name}] ERROR: {a.Data}"); };
        proc.Exited += (_, __) => { _headless.Remove(item.Path); Log($"[{item.Name}] exited with code {proc.ExitCode}"); };
        if (proc.Start())
        {
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            _headless[item.Path] = proc;
            Log($"Started {item.Name} headless.");
        }
    }

    private void BtnStopSelected_Click(object? sender, EventArgs e)
    {
        foreach (var item in lstRunProjects.CheckedItems.Cast<ProjectItem>())
        {
            if (_headless.TryGetValue(item.Path, out var proc))
            {
                TryKill(proc);
                _headless.Remove(item.Path);
                Log($"Stopped {item.Name}.");
            }
        }
    }

    private void BtnStopAll_Click(object? sender, EventArgs e)
    {
        foreach (var kv in _headless.ToList())
        {
            TryKill(kv.Value);
            _headless.Remove(kv.Key);
        }
        Log(chkRunTerminal.Checked
            ? "Stop All requested. Close terminal windows manually for running apps."
            : "Stopped all headless processes.");
    }

    private void BtnSelectAll_Click(object? sender, EventArgs e)
    {
        for (int i = 0; i < lstRunProjects.Items.Count; i++)
            lstRunProjects.SetItemChecked(i, true);
    }

    private void BtnDeselectAll_Click(object? sender, EventArgs e)
    {
        for (int i = 0; i < lstRunProjects.Items.Count; i++)
            lstRunProjects.SetItemChecked(i, false);
    }

    private async Task OpenBrowserWithDelayAsync(string url, TimeSpan delay)
    {
        await Task.Delay(delay);
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch { }
    }

    private void TryKill(Process proc)
    {
        try
        {
            if (proc.HasExited) return;
            proc.Kill(true);
            proc.WaitForExit(3000);
        }
        catch { }
    }

    private (string? Profile, string? Url) GetLaunchInfo(string projectDir)
    {
        try
        {
            var path = Path.Combine(projectDir, "Properties", "launchSettings.json");
            if (!File.Exists(path)) return (null, null);
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            if (!doc.RootElement.TryGetProperty("profiles", out var profiles)) return (null, null);
            var projectName = Path.GetFileName(projectDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            var candidates = new List<(string profile, string? url, int score)>();

            foreach (var profile in profiles.EnumerateObject())
            {
                var obj = profile.Value;
                var profileName = profile.Name;
                var command = obj.TryGetProperty("commandName", out var cmdEl) ? cmdEl.GetString() : null;
                var isIis = string.Equals(command, "IISExpress", StringComparison.OrdinalIgnoreCase) ||
                            profileName.Contains("IIS", StringComparison.OrdinalIgnoreCase);

                string? appUrls = obj.TryGetProperty("applicationUrl", out var appUrlsEl) ? appUrlsEl.GetString() : null;
                if (string.IsNullOrWhiteSpace(appUrls)) continue;
                var list = appUrls.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var httpsUrl = list.FirstOrDefault(u => u.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
                var first = httpsUrl ?? list.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(first)) continue;

                bool launchBrowser = obj.TryGetProperty("launchBrowser", out var lb) && lb.GetBoolean();
                string? launchUrl = obj.TryGetProperty("launchUrl", out var lu) ? lu.GetString() : null;
                if (launchBrowser && !string.IsNullOrWhiteSpace(launchUrl))
                {
                    if (!launchUrl.StartsWith('/')) launchUrl = "/" + launchUrl;
                    first = first.TrimEnd('/') + launchUrl;
                }

                int score = 0;
                if (!isIis) score += 5;
                if (!string.IsNullOrWhiteSpace(httpsUrl)) score += 2;
                if (string.Equals(profileName, projectName, StringComparison.OrdinalIgnoreCase)) score += 4;
                if (string.Equals(profileName, "https", StringComparison.OrdinalIgnoreCase)) score += 3;
                if (string.Equals(profileName, "http", StringComparison.OrdinalIgnoreCase)) score += 1;

                candidates.Add((profileName, first, score));
            }

            var chosen = candidates
                .OrderByDescending(c => c.score)
                .ThenBy(c => c.profile.StartsWith("IIS", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            return chosen.profile == null ? (null, null) : (chosen.profile, chosen.url);
        }
        catch { }
        return (null, null);
    }

    private (string FileName, string Arguments) BuildTerminalCommand(string title, string projectPath, string? profile, string workingDir)
    {
        var runArgs = new StringBuilder("dotnet run");
        if (!string.IsNullOrWhiteSpace(profile)) runArgs.Append($" --launch-profile \"{profile}\"");
        runArgs.Append(" --project \"").Append(projectPath).Append("\" --no-build");
        var runCommand = runArgs.ToString();

        var cmdExe = Environment.GetEnvironmentVariable("COMSPEC") ?? Path.Combine(Environment.SystemDirectory, "cmd.exe");
        var args2 = $"/c start \"{title}\" cmd /k {runCommand}";
        return (cmdExe, args2);
    }

    private void Log(string line)
    {
        if (txtRunLog.InvokeRequired)
        {
            txtRunLog.BeginInvoke(new Action<string>(Log), line);
            return;
        }
        txtRunLog.AppendText(line + Environment.NewLine);
    }

    private sealed class ProjectItem
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
        public override string ToString() => Display;
    }
}
