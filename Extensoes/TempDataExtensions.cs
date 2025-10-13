using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Json;

namespace Ecomerce.Extensoes
{
    public static class TempDataExtensions
    {
        // Método de extensão para colocar (armazenar) um objeto no TempData
        public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class
        {
            // Serializa o objeto para uma string JSON
            tempData[key] = JsonSerializer.Serialize(value);
        }

        // Método de extensão para obter (recuperar) um objeto do TempData
        public static T? Get<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            if (tempData.TryGetValue(key, out object? o))
            {
                // Desserializa a string JSON de volta para o objeto T
                var value = o as string;
                if (value != null)
                {
                    // Remove a chave para que a notificação não apareça na próxima página
                    tempData.Remove(key); 
                    return JsonSerializer.Deserialize<T>(value);
                }
            }
            return null;
        }
    }
}