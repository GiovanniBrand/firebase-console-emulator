using Firebase.Emulator.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Firebase.Emulator.Services.Impl
{
    internal class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthServiceOptions _options;

        public AuthService(HttpClient httpClient, IOptions<AuthServiceOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task GetOrCreateUserToken()
        {
            var loginPayload = new FirebaseAuthRequest
            {
                Email = _options.TestUser.Email,
                Password = _options.TestUser.Password,
                ReturnSecureToken = _options.TestUser.ReturnSecureToken
            };

            var signInUrl = _options.AuthEmulatorSignInUrl;
            var signUpUrl = _options.AuthEmulatorSignUpUrl;


            Console.WriteLine($"Tentando login como: {loginPayload.Email}...");

            var response = await _httpClient.PostAsJsonAsync(signInUrl, loginPayload);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Login realizado com sucesso!");
                await PrintTokenFromResponse(response);
                return;
            }

            var errorJson = await response.Content.ReadAsStringAsync();

            if (errorJson.Contains("EMAIL_NOT_FOUND"))
            {
                Console.WriteLine("Usuário não encontrado. Tentando criar a conta...");
                var signUpResponse = await _httpClient.PostAsJsonAsync(signUpUrl, loginPayload);

                if (signUpResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("Conta criada com sucesso!");
                    await PrintTokenFromResponse(signUpResponse);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Falha ao TENTAR CRIAR a conta:");
                    Console.WriteLine(await signUpResponse.Content.ReadAsStringAsync());
                    Console.ResetColor();
                }
            }
        }

        private async Task PrintTokenFromResponse(HttpResponseMessage response)
        {
            var authResponse = await response.Content.ReadFromJsonAsync<FirebaseAuthResponse>();
            if (authResponse == null || string.IsNullOrEmpty(authResponse.IdToken))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("A resposta não continha um IdToken (JWT)!");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(authResponse.IdToken);
            Console.WriteLine("SEU TOKEN JWT (Copie para o Postman)");
            Console.ResetColor();
        }
    }
}
