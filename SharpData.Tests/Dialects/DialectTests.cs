﻿using Xunit;
using SharpData;

namespace Sharp.Tests.Data.Databases {
    public abstract class DialectTests {
        protected string TABLE_NAME = "myTable";
        protected Dialect _dialect;
    }
}
