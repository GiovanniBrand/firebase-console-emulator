using Firebase.Emulator.Configurations;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Management;
using System.Net.Sockets;
using System.Runtime.Versioning;

namespace Firebase.Emulator.Services.Impl
{
    internal class EmulatorService : IEmulatorService
    {
        private Process _emulatorProcess;
        private readonly EmulatorOptions _options;
        private string? _rootExeName;
        private readonly ConcurrentQueue<string> _outputBuffer = new();

        public EmulatorService(IOptions<EmulatorOptions> options)
        {
            _options = options.Value;
        }

        public async Task<bool> StartFirebaseEmulator()
        {
            try
            {
                _emulatorProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "firebase.cmd",
                        Arguments = _options.Command,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    },
                    EnableRaisingEvents = true
                };

                _emulatorProcess.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        _outputBuffer.Enqueue(e.Data);
                    }
                };

                _emulatorProcess.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        _outputBuffer.Enqueue(e.Data);
                    }
                };

                _emulatorProcess.Start();
                _rootExeName = _emulatorProcess.ProcessName;
                _emulatorProcess.BeginOutputReadLine();
                _emulatorProcess.BeginErrorReadLine();

                Console.WriteLine("Emuladores iniciados em segundo plano.");

                await WaitForEmulatorAsync(_options.PortToCheck, _options.TimeoutSeconds);
                await WaitForAuthEmulatorReadyAsync(_options.TimeoutSeconds);

                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERRO!");
                Console.WriteLine("Não foi possível iniciar ou conectar aos emuladores.");
                Console.WriteLine($"Detalhe: {ex.Message}");
                Console.ResetColor();
                return false;
            }

        }
        [SupportedOSPlatform("windows")]
        public void StopFirebaseEmulator()
        {
            if (_emulatorProcess != null && !_emulatorProcess.HasExited)
            {
                Console.WriteLine("\nParando os emuladores do Firebase...");
                try
                {
                    _emulatorProcess.CloseMainWindow();
                    _emulatorProcess.WaitForExit(5000);

                    if (!_emulatorProcess.HasExited)
                    {
                        KillProcessTree(_emulatorProcess.Id);
                    }

                    Console.WriteLine("Emuladores parados.");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Erro ao tentar parar os emuladores: {ex.Message}");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine("\nEmuladores não estavam rodando ou já foram parados.");
            }
        }

        private async Task WaitForEmulatorAsync(int port, int timeoutSeconds)
        {
            Console.WriteLine($"Aguardando emulador na porta {port}...");
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed.TotalSeconds < timeoutSeconds)
            {
                try
                {
                    using (var tcpClient = new TcpClient())
                    {
                        await tcpClient.ConnectAsync("localhost", port);
                    }

                    Console.WriteLine("Porta aberta! Aguardando inicialização completa do Firebase...");
                    await Task.Delay(1000);

                    Console.WriteLine($"Emulador respondeu na porta {port} (levou {sw.Elapsed.TotalSeconds:F1}s).");
                    return;
                }
                catch (SocketException)
                {
                    await Task.Delay(500);
                }
            }
            throw new TimeoutException($"Timeout: Emulador na porta {port} não respondeu.");
        }

        [SupportedOSPlatform("windows")]
        private void KillProcessTree(int rootPid)
        {
            try
            {
                var root = Process.GetProcessById(rootPid);
                string rootName = root.ProcessName;

                foreach (var p in Process.GetProcesses())
                {
                    try
                    {
                        if (BelongsToTree(p, rootName))
                        {
                            if (!p.HasExited)
                            {
                                Console.WriteLine($"Matando: {p.ProcessName} [{p.Id}]");
                                p.Kill(true);
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        [SupportedOSPlatform("windows")]
        private bool BelongsToTree(Process process, string rootExe)
        {
            try
            {
                var current = process;

                while (true)
                {
                    if (current.ProcessName.Equals(rootExe, StringComparison.OrdinalIgnoreCase))
                        return true;

                    int parentPid = GetParentProcessId(current);
                    if (parentPid <= 0) break;

                    current = Process.GetProcessById(parentPid);
                }
            }
            catch { }

            return false;
        }

        [SupportedOSPlatform("windows")]
        private int GetParentProcessId(Process process)
        {
            using (var query = new ManagementObjectSearcher(
                       "SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = " + process.Id))
            {
                foreach (var obj in query.Get())
                    return Convert.ToInt32(obj["ParentProcessId"]);
            }
            return -1;
        }


        private async Task WaitForAuthEmulatorReadyAsync(int timeoutSeconds)
        {
            Console.WriteLine("Aguardando Firebase Auth Emulator (esperando saída do processo)...");

            var sw = Stopwatch.StartNew();

            while (sw.Elapsed.TotalSeconds < timeoutSeconds)
            {
                var items = _outputBuffer.ToArray();

                foreach (var line in items)
                {
                    if (line.IndexOf("All emulators ready", StringComparison.OrdinalIgnoreCase) >= 0
                        || line.IndexOf("All emulators ready", StringComparison.OrdinalIgnoreCase) >= 0
                        || line.IndexOf("auth: http://", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Console.WriteLine("Auth Emulator pronto (detectado via output do processo).");
                        return;
                    }
                }

                await Task.Delay(250);
            }

            throw new TimeoutException("Auth Emulator não inicializou a tempo (não foi detectada a linha 'Authentication Emulator started' na saída).");
        }
    }
}
