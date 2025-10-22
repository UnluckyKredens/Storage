namespace MagazineAPIApplication.Common.Exceptions;

public sealed class ForbiddenOperationException(string message) : Exception(message);
