using System;

using Microsoft.AspNetCore.Http;

namespace RestoRate.ServiceDefaults.HttpProblems;

public interface IExceptionProblemDetailsWriter
{
    bool CanWrite(Exception exception);
    void Write(ProblemDetailsContext context, Exception exception);
}
