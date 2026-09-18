namespace MagazineAPIApplication.Common.Exceptions;

public sealed class ResourceConflictException(string message) : Exception(message);
