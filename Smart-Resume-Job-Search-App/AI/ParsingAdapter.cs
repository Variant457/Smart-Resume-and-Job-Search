using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Smart_Resume_Job_Search_App.AI
{
    // Minimal subprocess bridge to call the Python resume parser during development
    public sealed class ParsingAdapter
    {
        private const string ScriptRelativePath = "AI/ParsingModel.py";

        public async Task<IReadOnlyList<ExtractedEntity>> TryParseAsync(string text, TimeSpan? timeout = null)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<ExtractedEntity>();

#if ANDROID || IOS
            // Not supported on mobile in this minimal bridge
            Debug.WriteLine("[ParsingAdapter] Python bridge is not supported on mobile platforms. Falling back.");
            return Array.Empty<ExtractedEntity>();
#else
            try
            {
                // Sanitize and normalize input before sending to Python (avoid TextEncodeInput errors on binary/invalid text)
                var clean = SanitizeForPython(text);
                if (string.IsNullOrWhiteSpace(clean))
                {
                    Debug.WriteLine("[ParsingAdapter] Input was not valid text after sanitization.");
                    return Array.Empty<ExtractedEntity>();
                }

                var scriptPath = ResolveScriptPath();
                if (!File.Exists(scriptPath))
                {
                    Debug.WriteLine($"[ParsingAdapter] Script not found at {scriptPath}");
                    return Array.Empty<ExtractedEntity>();
                }

                var interpreter = FindPythonInterpreter();
                if (!interpreter.HasValue)
                {
                    Debug.WriteLine("[ParsingAdapter] Python interpreter not found on PATH.");
                    return Array.Empty<ExtractedEntity>();
                }

                var (exe, usePy) = interpreter.Value;
                var args = usePy ? $"-3 \"{scriptPath}\"" : $"\"{scriptPath}\"";

                var psi = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = args,
                    WorkingDirectory = Path.GetDirectoryName(scriptPath)!,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    // Ensure UTF-8 across the pipe to avoid encoding issues
                    StandardInputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                    StandardOutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                    StandardErrorEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                };

                // Help Python treat stdio as UTF-8 on all platforms
                psi.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
                psi.EnvironmentVariables["PYTHONUTF8"] = "1";

                using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
                if (!proc.Start())
                {
                    Debug.WriteLine("[ParsingAdapter] Failed to start Python process.");
                    return Array.Empty<ExtractedEntity>();
                }

                // If the process exits immediately (e.g., ImportError), avoid writing to a closed pipe
                await Task.Delay(50);
                if (proc.HasExited)
                {
                    var earlyErr = await proc.StandardError.ReadToEndAsync();
                    Debug.WriteLine($"[ParsingAdapter] Python exited early ({proc.ExitCode}). Stderr: {earlyErr}");
                    return Array.Empty<ExtractedEntity>();
                }

                try
                {
                    await proc.StandardInput.WriteAsync(clean);
                    await proc.StandardInput.FlushAsync();
                    proc.StandardInput.Close();
                }
                catch (IOException ioex)
                {
                    // Child likely terminated and closed stdin (e.g., missing transformers/torch)
                    var stdErrEarly = await proc.StandardError.ReadToEndAsync();
                    Debug.WriteLine($"[ParsingAdapter] Failed writing to Python stdin (pipe closed). {ioex}\nStderr: {stdErrEarly}");
                    return Array.Empty<ExtractedEntity>();
                }

                var to = timeout ?? TimeSpan.FromSeconds(30);
                var waitTask = Task.Run(() => proc.WaitForExit());
                var completed = await Task.WhenAny(waitTask, Task.Delay(to)) == waitTask;
                if (!completed)
                {
                    try { proc.Kill(entireProcessTree: true); } catch { }
                    Debug.WriteLine("[ParsingAdapter] Python process timed out.");
                    return Array.Empty<ExtractedEntity>();
                }

                var stdOut = await proc.StandardOutput.ReadToEndAsync();
                var stdErr = await proc.StandardError.ReadToEndAsync();

                if (proc.ExitCode != 0)
                {
                    Debug.WriteLine($"[ParsingAdapter] Python error ({proc.ExitCode}): {stdErr}");
                    return Array.Empty<ExtractedEntity>();
                }

                if (string.IsNullOrWhiteSpace(stdOut))
                    return Array.Empty<ExtractedEntity>();

                try
                {
                    using var doc = JsonDocument.Parse(stdOut);
                    if (doc.RootElement.ValueKind != JsonValueKind.Array)
                        return Array.Empty<ExtractedEntity>();

                    var list = new List<ExtractedEntity>();
                    foreach (var el in doc.RootElement.EnumerateArray())
                    {
                        var label = el.TryGetProperty("label", out var l) ? l.GetString() ?? string.Empty : string.Empty;
                        var txt = el.TryGetProperty("text", out var t) ? t.GetString() ?? string.Empty : string.Empty;
                        var start = el.TryGetProperty("start", out var s) && s.TryGetInt32(out var si) ? si : -1;
                        if (!string.IsNullOrWhiteSpace(label) && !string.IsNullOrWhiteSpace(txt))
                            list.Add(new ExtractedEntity(label, txt, start));
                    }
                    return list;
                }
                catch (Exception jex)
                {
                    Debug.WriteLine($"[ParsingAdapter] Failed to parse JSON: {jex}\nRaw: {stdOut}");
                    return Array.Empty<ExtractedEntity>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ParsingAdapter] Exception: {ex}");
                return Array.Empty<ExtractedEntity>();
            }
#endif
        }

        private static string ResolveScriptPath()
        {
            // Prefer the file next to app binaries (copied by csproj for desktop)
            var baseDir = AppContext.BaseDirectory;
            var candidate = Path.Combine(baseDir, ScriptRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(candidate))
                return candidate;

            // Dev fallback: probe up to repo root
            var probe = Path.Combine(baseDir, "..", "..", "..", ScriptRelativePath.Replace('/', Path.DirectorySeparatorChar));
            return Path.GetFullPath(probe);
        }

        private static (string exe, bool usePy)? FindPythonInterpreter()
        {
            var candidates = new List<(string exe, bool usePy)>();
#if WINDOWS
            candidates.Add(("py", true));
#endif
            candidates.Add(("python", false));
            candidates.Add(("python3", false));

            foreach (var c in candidates)
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = c.exe,
                        Arguments = c.usePy ? "-3 --version" : "--version",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };
                    using var p = Process.Start(psi);
                    if (p == null) continue;
                    if (!p.WaitForExit(2000)) { try { p.Kill(); } catch { } continue; }
                    if (p.ExitCode == 0)
                        return c;
                }
                catch
                {
                    // ignore and try next
                }
            }
            return null;
        }

        private static string SanitizeForPython(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            // Walk by Unicode scalar values to strip invalid UTF-16 sequences and control chars
            var sb = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length;)
            {
                if (!Rune.TryGetRuneAt(text, i, out var rune))
                {
                    // Invalid surrogate or sequence; replace with space and advance one
                    sb.Append(' ');
                    i++;
                    continue;
                }

                i += rune.Utf16SequenceLength;

                // Drop most control chars except common whitespace
                if (Rune.IsControl(rune))
                {
                    if (rune.Value == '\n' || rune.Value == '\r' || rune.Value == '\t')
                    {
                        sb.Append(rune.ToString());
                    }
                    else
                    {
                        sb.Append(' ');
                    }
                    continue;
                }

                sb.Append(rune.ToString());
            }

            var cleaned = sb.ToString();

            string norm;
            try
            {
                norm = cleaned.Normalize(NormalizationForm.FormKC);
            }
            catch
            {
                // If normalization fails for any reason, fall back to the cleaned string
                norm = cleaned;
            }

            norm = norm.Trim();

            // If the text looks mostly non-printable, bail out early
            if (!LooksLikeHumanText(norm))
                return string.Empty;

            // Keep it bounded (tokenizer will truncate too)
            const int MaxChars = 200_000; // ~200 KB of text
            if (norm.Length > MaxChars)
                norm = norm.Substring(0, MaxChars);

            return norm;
        }

        private static bool LooksLikeHumanText(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            int printable = 0;
            int total = 0;
            foreach (var ch in s)
            {
                total++;
                if (char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch) || char.IsPunctuation(ch) || char.IsSymbol(ch))
                    printable++;
            }
            // Require at least60% printable chars
            return total > 0 && printable * 100 / total >= 60;
        }
    }
}
