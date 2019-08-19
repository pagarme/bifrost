using System;
using Type = PagarMe.Bifrost.Data.PaymentRequest.Type;

namespace PagarMe.Bifrost.Data
{
    public static class TypeExtension
    {
        internal static Type[] GetNextAllowed(this Type type)
        {
            switch (type)
            {
                case Type.UnknownCommand:
                    return new[] { Type.Initialize };

                case Type.Initialize:
                    return new[] { Type.Initialize /*answers already initialized*/, Type.Process };

                case Type.Process:
                    return new[] { Type.Finish, Type.Initialize };

                case Type.Finish:
                    return new[] { Type.Process, Type.Initialize };

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
