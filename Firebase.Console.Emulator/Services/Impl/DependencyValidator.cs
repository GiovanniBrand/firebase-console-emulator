using Firebase.Emulator.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Firebase.Emulator.Services.Impl
{
    internal class DependencyValidator : IDependencyValidator
    {
        private readonly PathFirebaseConfig pathFirebase;

        public DependencyValidator(IOptions<PathFirebaseConfig> pathFirebase)
        {
            this.pathFirebase = pathFirebase.Value;
        }

        public async Task<bool> ValidateAsync()
        {
            Console.WriteLine("Verificando dependências (Node.js e Firebase Tools)...");
            CreateFirebasePath();

            if (!await IsCommandAvailableAsync("npm", "-v"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("--- ERRO DE DEPENDÊNCIA ---");
                Console.WriteLine("Node.js (npm) não foi encontrado no seu PATH.");
                Console.WriteLine("Por favor, instale o Node.js LTS em: https://nodejs.org/");
                Console.ResetColor();
                return false;
            }

            if (!await IsCommandAvailableAsync("firebase", "--version"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("--- ERRO DE DEPENDÊNCIA ---");
                Console.WriteLine("'firebase-tools' não foi encontrado no seu PATH.");
                Console.WriteLine("Por favor, instale-o globalmente rodando o comando abaixo");
                Console.WriteLine("em um terminal (como Administrador):");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("npm install -g firebase-tools");
                Console.ResetColor();
                return false;
            }

            Console.WriteLine("Dependências verificadas com sucesso.");
            return true;
        }

        private async Task<bool> IsCommandAvailableAsync(string fileName, string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {fileName} {arguments}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            try
            {
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string errors = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"DEBUG (Comando falhou): {fileName} {arguments} -> {errors}");
                }
                return process.ExitCode == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void CreateFirebasePath()
        {
            if (!Directory.Exists(pathFirebase.Path))
                Directory.CreateDirectory(pathFirebase.Path);
        }
    }
}
