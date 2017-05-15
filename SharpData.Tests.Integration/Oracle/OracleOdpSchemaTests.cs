﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Sharp.Data;
using Sharp.Data.Databases;
using Sharp.Tests.Databases.Data;

namespace Sharp.Tests.Databases.Oracle {
	[TestFixture]
	public class OracleOdpSchemaTests : DataClientSchemaTests {

		[SetUp]
		public void SetUp() {
            _dataClient = DBBuilder.GetDataClient(DataProviderNames.OracleOdp);
		}

	}
}
