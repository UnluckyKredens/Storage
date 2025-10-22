namespace MagazineAPIApplication.Common.Exceptions;

public sealed class CommandValidationException(string message) : Exception(message);
