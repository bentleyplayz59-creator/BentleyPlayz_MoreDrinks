using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace BentleyPlayz.MoreDrinks;

internal sealed class ModEntry : Mod
{
    private const string ContentPackId = "BentleyPlayz.MoreDrinks.Content";
    private const long MaxLogBytes = 2 * 1024 * 1024;
    private readonly object logLock = new();
    private string diagnosticPath = "";
    private string lastSmapiLog = "";
    private bool warnedAboutSmapiLog;

    public override void Entry(IModHelper helper)
    {
        this.diagnosticPath = Path.Combine(helper.DirectoryPath, "diagnostics.log");

        AppDomain.CurrentDomain.UnhandledException += this.OnUnhandledException;
        TaskScheduler.UnobservedTaskException += this.OnUnobservedTaskException;

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
        helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;

        this.WriteDiagnostic("INFO", "========== BentleyPlayz More Drinks diagnostic session started ==========");
        this.WriteDiagnostic("INFO", $"Mod version: {this.ModManifest.Version}");
        this.WriteDiagnostic("INFO", $"Runtime: {Environment.Version}; OS: {Environment.OSVersion}");
        this.WriteDiagnostic("INFO", $"Diagnostic file: {this.diagnosticPath}");
        this.WriteDiagnostic("INFO", $"SMAPI log source: {Path.Combine(Constants.LogDir, "SMAPI-latest.txt")}");
        this.Monitor.Log("BentleyPlayz More Drinks loaded. Automatic diagnostics are being written to Code/diagnostics.log, including the live SMAPI log stream.", LogLevel.Info);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.RunSafely("GameLaunched diagnostics", () =>
        {
            bool loaded = this.Helper.ModRegistry.IsLoaded(ContentPackId);
            string message = loaded
                ? $"Content Patcher pack '{ContentPackId}' detected."
                : $"Content Patcher pack '{ContentPackId}' was NOT detected. Check that the combined mod's Content folder is installed beside Code.";
            this.WriteDiagnostic(loaded ? "INFO" : "ERROR", message);
            this.Monitor.Log(message, loaded ? LogLevel.Info : LogLevel.Error);
        });
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        this.WriteDiagnostic("INFO", "Save loaded. Runtime diagnostics are active for this session.");
        this.RunSafely("HunnyBwat shoutout GUI", () =>
        {
            Game1.drawObjectDialogue("SHOUTOUT TO HUNNYBWAT!\n\nA special shoutout to HunnyBwat, the Twitch streamer, for inspiring this drinks collection. Thanks for supporting the Stardew Valley community!");
            this.WriteDiagnostic("INFO", "Displayed the HunnyBwat startup shoutout GUI.");
        });
    }

    private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        this.WriteDiagnostic("INFO", "Returned to title. Save session ended.");
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        this.saloonDoorHoursPatched = false;
        this.saloonDoorHoursPatched = this.TryApplySaloonDoorHours();
    }

    private void OnWarped(object? sender, WarpedEventArgs e)
    {
        if (string.Equals(e.NewLocation.NameOrUniqueNam
        {
            string smapiPath = Path.Combine(Constants.LogDir, "SMAPI-latest.txt");
            if (!File.Exists(smapiPath))
                return;

            string current = File.ReadAllText(smapiPath, Encoding.UTF8);
            string newText;
            if (current.StartsWith(this.lastSmapiLog, StringComparison.Ordinal))
                newText = current.Substring(this.lastSmapiLog.Length);
            else
            {
                this.WriteDiagnostic("WARN", "SMAPI log was reset or rotated; starting a fresh capture.");
                newText = current;
            }

            this.lastSmapiLog = current;
            if (!string.IsNullOrEmpty(newText))
                this.WriteDiagnostic("SMAPI", newText.TrimEnd());
            this.warnedAboutSmapiLog = false;
        });
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        string details = e.ExceptionObject is Exception ex
            ? ex.ToString()
            : Convert.ToString(e.ExceptionObject) ?? "Unknown unhandled exception.";
        this.WriteDiagnostic("FATAL", $"Unhandled process exception. IsTerminating={e.IsTerminating}{Environment.NewLine}{details}");
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        this.WriteDiagnostic("ERROR", $"Unobserved task exception:{Environment.NewLine}{e.Exception}");
    }

    private void RunSafely(string operation, Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            if (operation != "SMAPI log capture" || !this.warnedAboutSmapiLog)
            {
                this.WriteDiagnostic("ERROR", $"{operation} failed:{Environment.NewLine}{ex}");
                this.Monitor.Log($"{operation} failed. Details were written to diagnostics.log.", LogLevel.Error);
            }
            if (operation == "SMAPI log capture")
                this.warnedAboutSmapiLog = true;
        }
    }

    private void WriteDiagnostic(string level, string message)
    {
        string entry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC] [{level}] {message}{Environment.NewLine}";
        lock (this.logLock)
        {
            try
            {
                string? directory = Path.GetDirectoryName(this.diagnosticPath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                FileInfo existing = new(this.diagnosticPath);
                if (existing.Exists && existing.Length > MaxLogBytes)
                {
                    string previous = this.diagnosticPath + ".previous";
                    if (File.Exists(previous))
                        File.Delete(previous);
                    File.Move(this.diagnosticPath, previous);
                }

                File.AppendAllText(this.diagnosticPath, entry, Encoding.UTF8);
            }
            catch (Exception logException)
            {
                this.Monitor.Log($"Could not write diagnostics.log: {logException.Message}", LogLevel.Error);
            }
        }
    }
}
