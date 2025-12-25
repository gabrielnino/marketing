using Application.Result;
using Infrastructure.Constants;

namespace Infrastructure.Utilities
{
    public class GuidValidator
    {
        public static Operation<string> HasGuid(string id)
        {
            bool isSuccess = Guid.TryParse(id, out _);
            if (!isSuccess)
            {
                var business = new BusinessStrategy<string>();
                var invalidGuidMessage = Message.GuidValidator.InvalidGuid;
                return OperationStrategy<string>.Fail(invalidGuidMessage, business);
            }

            return Operation<string>.Success(id, Message.GuidValidator.Success);
        }
    }
}
