namespace Errs.Tests;

public class ErrTest
{
    record IndexOutOfRange(String Name, int Index, int Min, int Max);

    public class TestConstructor
    {
        [Fact]
        public void with_Record_reason()
        {
            var err = new Err(new IndexOutOfRange("data", 4, 0, 3));
            var reason = (IndexOutOfRange)err.Reason;
            Assert.Equal(reason.Name, "data");
            Assert.Equal(reason.Index, 4);
            Assert.Equal(reason.Min, 0);
            Assert.Equal(reason.Max, 3);
            Assert.Null(err.InnerException);
            Assert.Equal(err.Message,
                "IndexOutOfRange { Name = data, Index = 4, Min = 0, Max = 3 }");
        }

        enum Reasons
        {
            FailToDoSomething,
        }

        [Fact]
        public void with_enum_reason()
        {
            var err = new Err(Reasons.FailToDoSomething);
            var reason = (Reasons)err.Reason;
            Assert.Equal(reason.ToString(), "FailToDoSomething");
            Assert.Null(err.InnerException);
            Assert.Equal(err.Message, "FailToDoSomething");
        }

        [Fact]
        public void with_string_reason()
        {
            var err = new Err("FailToDoSomething");
            var reason = (String)err.Reason;
            Assert.Equal(reason, "FailToDoSomething");
            Assert.Null(err.InnerException);
            Assert.Equal(err.Message, "FailToDoSomething");
        }

        [Fact]
        public void with_reason_but_reason_is_null()
        {
            try
            {
#pragma warning disable CS8625
                new Err(null);
#pragma warning restore CS8625
            }
            catch (ArgumentNullException e)
            {
                Assert.Equal(e.Message, "Value cannot be null. (Parameter 'reason')");
            }
        }

        [Fact]
        public void with_reason_and_innerException()
        {
            var cause = new IndexOutOfRangeException("4 is out of range: 0-3");
            var err = new Err(new IndexOutOfRange("data", 4, 0, 3), cause);
            var reason = (IndexOutOfRange)err.Reason;
            Assert.Equal(reason.Name, "data");
            Assert.Equal(reason.Index, 4);
            Assert.Equal(reason.Min, 0);
            Assert.Equal(reason.Max, 3);
            Assert.Equal(err.InnerException, cause);
            Assert.Equal(err.Message,
                "IndexOutOfRange { Name = data, Index = 4, Min = 0, Max = 3 }");
        }

        [Fact]
        public void with_reason_and_innerException_but_reason_is_null()
        {
            var cause = new IndexOutOfRangeException("4 is out of range: 0-3");
            try
            {
#pragma warning disable CS8625
                var err = new Err(null, cause);
#pragma warning restore CS8625
            }
            catch (ArgumentNullException e)
            {
                Assert.Equal(e.Message, "Value cannot be null. (Parameter 'reason')");
            }
        }

        [Fact]
        public void with_reason_and_innerException_but_innerException_is_null()
        {
            var err = new Err(new IndexOutOfRange("data", 4, 0, 3), null);
            var reason = (IndexOutOfRange)err.Reason;
            Assert.Equal(reason.Name, "data");
            Assert.Equal(reason.Index, 4);
            Assert.Equal(reason.Min, 0);
            Assert.Equal(reason.Max, 3);
            Assert.Null(err.InnerException);
            Assert.Equal(err.Message,
                "IndexOutOfRange { Name = data, Index = 4, Min = 0, Max = 3 }");
        }
    }

    public class TestThrow
    {
        [Fact]
        public void identify_reason_with_is()
        {
            var err = new Err(new IndexOutOfRange("data", 4, 0, 3));
            if (err.Reason is IndexOutOfRange reason)
            {
                Assert.Equal(reason.Name, "data");
                Assert.Equal(reason.Index, 4);
                Assert.Equal(reason.Min, 0);
                Assert.Equal(reason.Max, 3);
            }
            else
            {
                Assert.Fail();
            }
        }

        [Fact]
        public void identify_Record_reason_with_switch_expression()
        {
            var err = new Err(new IndexOutOfRange("data", 4, 0, 3));
            var name = err.Reason switch
            {
                IndexOutOfRange reason => reason.Name,
                _ => "",
            };
            Assert.Equal(name, "data");

            switch (err.Reason)
            {
                case IndexOutOfRange reason:
                    Assert.Equal(reason.Name, "data");
                    Assert.Equal(reason.Index, 4);
                    Assert.Equal(reason.Min, 0);
                    Assert.Equal(reason.Max, 3);
                    break;

                default:
                    Assert.Fail();
                    break;
            }
        }

        enum Reasons
        {
            FailToDoSomething,
            InvalidValue,
        }

        [Fact]
        public void identify_enum_reason_with_switch_expression()
        {
            var err = new Err(Reasons.FailToDoSomething);

            var s = err.Reason switch
            {
                Reasons enm => enm switch
                {
                    Reasons.FailToDoSomething => "fail to do something",
                    Reasons.InvalidValue => "invalid value",
                    _ => "unknown",
                },
                _ => "unknown",
            };
            Assert.Equal(s, "fail to do something");

            switch (err.Reason)
            {
                case Reasons enm:
                    switch (enm)
                    {
                        case Reasons.FailToDoSomething:
                            break;
                        case Reasons.InvalidValue:
                            Assert.Fail();
                            break;
                        default:
                            Assert.Fail();
                            break;
                    }
                    break;

                default:
                    break;
            }
        }
    }

    public class TestGetter
    {
        [Fact]
        public void reason()
        {
            var err = new Err(new IndexOutOfRange("data", 4, 0, 3));
            Assert.True(err.Reason is IndexOutOfRange);

            var reason = (IndexOutOfRange)err.Reason;
            Assert.Equal(reason.Name, "data");
            Assert.Equal(reason.Index, 4);
            Assert.Equal(reason.Min, 0);
            Assert.Equal(reason.Max, 3);
        }

        [Fact]
        public void innerException()
        {
            var err = new Err(new IndexOutOfRange("data", 4, 0, 3));
            Assert.Null(err.InnerException);

            var cause = new IndexOutOfRangeException("4 is out of range: 0-3");
            err = new Err(new IndexOutOfRange("data", 4, 0, 3), cause);
            Assert.Equal(err.InnerException, cause);
        }

        [Fact]
        public void file()
        {
            var err = new Err(new IndexOutOfRange("data", 4, 0, 3));
            Assert.Equal(err.File, "ErrTest.cs");
        }

        [Fact]
        public void line()
        {
            var err = new Err(new IndexOutOfRange("data", 4, 0, 3));
            Assert.Equal(err.Line, 235);
        }
    }

    public class TestMessage
    {
        [Fact]
        public void with_no_innerException()
        {
            var err = new Err(new IndexOutOfRange("data", 4, 0, 3));
            Assert.Equal(err.Message, "IndexOutOfRange { Name = data, Index = 4, Min = 0, Max = 3 }");
        }

        [Fact]
        public void with_innerException()
        {
            var cause = new IndexOutOfRangeException("4 is out of range: 0-3");
            var err = new Err(new IndexOutOfRange("data", 4, 0, 3), cause);
            Assert.Equal(err.Message, "IndexOutOfRange { Name = data, Index = 4, Min = 0, Max = 3 }");
        }
    }

    public class TestToString
    {
        [Fact]
        public void with_reason()
        {
            var err = new Err(new IndexOutOfRange("data", 4, 0, 3));
            Assert.Equal(err.ToString(), "Errs.Err { reason = Errs.Tests.ErrTest+IndexOutOfRange IndexOutOfRange { Name = data, Index = 4, Min = 0, Max = 3 }, file = ErrTest.cs, line = 263 }");
        }

        [Fact]
        public void with_reason_and_innerException()
        {
            var cause = new IndexOutOfRangeException("4 is out of range: 0-3");
            var err = new Err(new IndexOutOfRange("data", 4, 0, 3), cause);
            Assert.Equal(err.ToString(), "Errs.Err { reason = Errs.Tests.ErrTest+IndexOutOfRange IndexOutOfRange { Name = data, Index = 4, Min = 0, Max = 3 }, file = ErrTest.cs, line = 271, cause = System.IndexOutOfRangeException: 4 is out of range: 0-3 }");
        }
    }
}
