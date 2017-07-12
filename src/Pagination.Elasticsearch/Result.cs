using System;

namespace Pagination.Elasticsearch
{
    public struct Result<T>
    {
        public string ErrorMessage { get; private set; }
        public T Value { get; private set; }

        public static Result<T> Success(T value) => new Result<T> { Value = value };
        public static Result<T> Error(string errorMessage) => new Result<T> { ErrorMessage = errorMessage };
        public static Result<T> Error<U>(Result<U> result) => Error(result.ErrorMessage);

        public static implicit operator Result<T>(T value) => Success(value);
        public static implicit operator Result<T>(Exception exception) => Error(exception.Message);
        public static implicit operator bool(Result<T> value) => value.IsSuccess;

        public bool IsSuccess => ErrorMessage == null;
    }
}
