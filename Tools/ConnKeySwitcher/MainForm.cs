using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        cboKey.Items.AddRange(KnownKeys.Cast<object>().ToArray());
        cboKey.SelectedIndex = Array.IndexOf(KnownKeys, "HuyConnection");

        try
        {
            // Default to repo root if running from it
            txtRoot.Text = FindRepoRoot() ?? Environment.CurrentDirectory;
        }
        catch
        {
            txtRoot.Text = Environment.CurrentDirectory;
        }
    }

    private static string? FindRepoRoot()
    {
        var dir = new DirectoryInfo(Environment.CurrentDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "DOS.sln"))) return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }

    private void Browse()
    {
        folderDialog.SelectedPath = Directory.Exists(txtRoot.Text) ? txtRoot.Text : Environment.CurrentDirectory;
        if (folderDialog.ShowDialog(this) == DialogResult.OK)
        {
            txtRoot.Text = folderDialog.SelectedPath;
        }
    }

    private IEnumerable<string> EnumerateProgramFiles()
    {
        var root = txtRoot.Text.Trim();
        if (!Directory.Exists(root)) yield break;

        foreach (var file in Directory.EnumerateFiles(root, "Program.cs", SearchOption.AllDirectories))
        {
            // skip bin/obj
            if (file.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                continue;
            if (file.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                continue;
            yield return file;
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

    private void Scan()
    {
        grid.Rows.Clear();
        var files = EnumerateProgramFiles().ToList();
        int i = 0;
        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            var matches = _pattern.Matches(text);
            if (matches.Count == 0) { i++; continue; }

            var keys = matches.Select(m => m.Groups["key"].Value).ToList();
            var unique = string.Join(", ", keys.Distinct());
            var currentKey = keys.LastOrDefault();
            var target = cboKey.SelectedItem?.ToString() ?? string.Empty;
            bool willChange = keys.Any(k => !string.Equals(k, target, StringComparison.Ordinal));

            grid.Rows.Add(file, unique, matches.Count, willChange);
            i++;
            SetStatus($"Scanning {i}/{files.Count} files...", true, (int)(100.0 * i / files.Count));
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

        int filesChanged = 0, locations = 0;
        var files = EnumerateProgramFiles().ToList();
        int idx = 0;
        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            if (!_pattern.IsMatch(text)) continue;

            var replaced = _pattern.Replace(text, m =>
            {
                locations++;
                return $"GetConnectionString(\"{selectedKey}\")";
            });



            idx++;
            SetStatus($"Applying {idx}/{files.Count} files...", true, (int)(100.0 * idx / files.Count));
            Application.DoEvents();
        }

        Scan();
        SetStatus("Done.");
        MessageBox.Show(this, $"Updated {filesChanged} files, {locations} locations.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnBrowse_Click(object? sender, EventArgs e) => Browse();
    private void BtnScan_Click(object? sender, EventArgs e) => Scan();
    private void BtnApply_Click(object? sender, EventArgs e) => Apply();

    private void grid_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {

    }
}