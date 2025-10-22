using System.Collections;
using System.Net.Mail;
using System.Reflection;
using MagazineAPIApplication.Common.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MagazineAPI.Services;

public sealed class RequestValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var argument in context.ActionArguments.Values)
            if (argument is not null && argument.GetType().Namespace?.StartsWith("Magazine", StringComparison.Ordinal) == true)
                Validate(argument, 0);
    }

    public void OnActionExecuted(ActionExecutedContext context) { }

    private static void Validate(object value, int depth)
    {
        if (depth > 8) throw new CommandValidationException("Zbyt złożone dane żądania.");
        foreach (var property in value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var field = property.GetValue(value);
            var name = property.Name;
            if (field is string text)
            {
                var max = name switch
                {
                    "Login" or "FirstName" or "LastName" or "Sku" or "Barcode" or "Code" or "InvoiceNumber" or "PaperDocumentNumber" => 100,
                    "Password" or "CurrentPassword" or "NewPassword" => 128,
                    "CustomerName" or "Name" => 200,
                    "Email" or "CustomerEmail" => 255,
                    "Phone" or "CustomerPhone" or "LocationCode" => 50,
                    "Address" or "DeliveryAddress" => 500,
                    "Description" when value.GetType().Name.Contains("Location", StringComparison.Ordinal) => 500,
                    "IdempotencyKey" => 64,
                    _ => 1000
                };
                if (text.Length > max) throw new CommandValidationException($"Pole {name} może mieć najwyżej {max} znaków.");
                if (name is "Email" or "CustomerEmail" && text.Length > 0 &&
                    (!MailAddress.TryCreate(text.Trim(), out var address) || address.Address != text.Trim()))
                    throw new CommandValidationException($"Pole {name} nie zawiera poprawnego adresu e-mail.");
            }
            else if (field is decimal number)
            {
                var scale = name.Contains("Price", StringComparison.Ordinal) ? 2 : 3;
                var maximum = scale == 2 ? 9999999999999999.99m : 999999999999999.999m;
                if ((number > maximum || number < -maximum) || decimal.Round(number, scale) != number)
                    throw new CommandValidationException($"Pole {name} ma nieprawidłowy zakres lub precyzję (maksymalnie {scale} miejsca po przecinku).");
            }
            else if (field is IEnumerable collection)
            {
                var count = 0;
                foreach (var item in collection)
                {
                    if (++count > 100) throw new CommandValidationException("Żądanie może zawierać najwyżej 100 pozycji.");
                    if (item is null) throw new CommandValidationException("Lista nie może zawierać pustych pozycji.");
                    if (item.GetType().Namespace?.StartsWith("Magazine", StringComparison.Ordinal) == true) Validate(item, depth + 1);
                }
            }
        }
    }
}
