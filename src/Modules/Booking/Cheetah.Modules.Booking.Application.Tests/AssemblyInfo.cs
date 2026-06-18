using System.Runtime.CompilerServices;

// Moq (Castle DynamicProxy) должен видеть internal тестовые типы (TestBooking и т.п.),
// чтобы создавать прокси для IRepository<TestBooking, Guid> и пр.
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
