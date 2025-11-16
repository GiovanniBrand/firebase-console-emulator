using System.ComponentModel.DataAnnotations;

namespace Firebase.Emulator.Services.Impl
{
    internal class OrchestratorService : IOrchestratorService
    {
        private readonly IEmulatorService _emulatorService;
        private readonly IAuthService _authService;
        private readonly IDependencyValidator _validator;

        public OrchestratorService(
            IEmulatorService emulatorService,
            IAuthService authService,
            IDependencyValidator validator)
        {
            _emulatorService = emulatorService;
            _authService = authService;
            _validator = validator;
        }

        public async Task RunAsync()
        {
            bool houveErro = false;
            try
            {
                if (!await _validator.ValidateAsync())
                {
                    throw new Exception("Falha na validação de dependências.");
                }

                if (!await _emulatorService.StartFirebaseEmulator())
                {
                    throw new Exception("Falha ao iniciar o emulador. Verifique os logs acima.");
                }

                await _authService.GetOrCreateUserToken();
            }
            catch (Exception ex)
            {
                houveErro = true;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nOcorreu um erro inesperado: {ex.Message}");
                Console.ResetColor();

                Console.WriteLine("\nA aplicação será encerrada em 10 segundos...");
                await Task.Delay(10000);
            }
            finally
            {
                if (houveErro)
                {
                    Console.WriteLine("Encerrando automaticamente...");
                }
                else
                {
                    Console.WriteLine("\n=======================================================");
                    Console.WriteLine("✅ Os emuladores (Auth, Firestore) estão rodando.");
                    Console.WriteLine("Pressione Enter neste console para FECHAR e DERRUBAR OS EMULADORES.");
                    Console.ReadLine();
                }

                _emulatorService.StopFirebaseEmulator();
            }
        }
    }
}
