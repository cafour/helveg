using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Helveg;

public static class HelvegDefaults
{
    public static readonly JsonSerializerOptions JsonOptions;

    static HelvegDefaults()
    {
        JsonOptions = new JsonSerializerOptions();
    }
}
