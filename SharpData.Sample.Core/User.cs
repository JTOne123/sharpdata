﻿namespace SharpData.Sample {
    public class User {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserInsert {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
