using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SQS.Model;

namespace AmazonSqsSubscription.Extensions;

internal static class SqsMessageTypeAttributeExtensions
{
    private const string AttributeName = "MessageType";

    public static string GetMessageTypeAttributeValue(this Dictionary<string, MessageAttributeValue> attributes)
    {
        return attributes.SingleOrDefault(x => x.Key == AttributeName).Value?.StringValue;
    }
}
