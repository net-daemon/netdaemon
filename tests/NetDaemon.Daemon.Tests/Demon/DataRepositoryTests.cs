using JoySoftware.HomeAssistant.NetDaemon.Common;
using JoySoftware.HomeAssistant.NetDaemon.Daemon.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NetDaemon.Daemon.Tests.Daemon
{
    public class DataRepositoryTests : DaemonHostTestBase
    {
        public static readonly string DataReposityryPath =
            Path.Combine(AppContext.BaseDirectory, "datarepository");

        public DataRepositoryTests() : base()
        {
        }

        [Fact]
        public async Task GetNonExistantValueShouldReturnNull()
        {
            // ARRANGE
            var daemon = DefaultDaemonHost;

            // ACT
            var data = await daemon.GetDataAsync<string>("not_exists");
            // ASSERT
            Assert.Null(data);
        }

        [Fact]
        public async Task SavedDataShouldReturnSameDataUsingExpando()
        {
            // ARRANGE
            var daemon = DefaultDaemonHost;
            dynamic data = new ExpandoObject();
            data.Item = "Some data";

            // ACT
            await daemon.SaveDataAsync("data_exists", data);
            var collectedData = await daemon.GetDataAsync<ExpandoObject>("data_exists");

            // ASSERT
            Assert.Equal(data, collectedData);
        }

        [Fact]
        public async Task GetDataShouldReturnCachedValue()
        {
            // ARRANGE
            var daemon = DefaultDaemonHost;
            // ACT

            await daemon.SaveDataAsync("GetDataShouldReturnCachedValue_id", "saved data");

            await daemon.GetDataAsync<string>("GetDataShouldReturnCachedValue_id");

            // ASSERT
            DefaultDataRepositoryMock.Verify(n => n.Get<string>(It.IsAny<string>()), Times.Never);
            DefaultDataRepositoryMock.Verify(n => n.Save<string>(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RepositoryLoadSavedDataUsingExpando()
        {
            // ARRANGE
            var dataRepository = new DataRepository(DataReposityryPath);
            dynamic dataBeingSaved = new FluentExpandoObject(false, true);
            var now = DateTime.Now;
            dataBeingSaved.SomeString = "this data should be saved!";
            dataBeingSaved.SomeInt = 123456;
            dataBeingSaved.SomeFloat = 1.23456;
            dataBeingSaved.SomeDateTime = now;

            // ACT
            await dataRepository.Save<IDictionary<string, object>>("RepositoryLoadSavedData_id", dataBeingSaved);

            var dataReturned = await dataRepository.Get<IDictionary<string, object>>("RepositoryLoadSavedData_id");
            var returnedFluentExpandoObject = new FluentExpandoObject();
            returnedFluentExpandoObject.CopyFrom(dataReturned!);

            dynamic dynamicDataReturned = returnedFluentExpandoObject;
            // ASSERT
            Assert.Equal(dataBeingSaved.SomeString, dynamicDataReturned!.SomeString);
            Assert.Equal(dataBeingSaved.SomeInt, dynamicDataReturned!.SomeInt);
            Assert.Equal(dataBeingSaved.SomeFloat, dynamicDataReturned!.SomeFloat);
            Assert.Equal(dataBeingSaved.SomeDateTime, dynamicDataReturned!.SomeDateTime);
        }

        [Fact]
        public async Task RepositoryShouldLoadSavedDataUsingDto()
        {
            // ARRANGE
            var dataRepository = new DataRepository(DataReposityryPath);
            var storeData = new TestStorage
            {
                AString = "Some String",
                AInt = 12345,
                ADateTime = DateTime.Now
            };
            // ACT
            await dataRepository.Save<TestStorage>("RepositoryShouldLoadSavedDataUsingDto_id", storeData);

            var dataReturned = await dataRepository.Get<TestStorage>("RepositoryShouldLoadSavedDataUsingDto_id");

            // ASSERT
            Assert.Equal(storeData.AString, dataReturned!.AString);
            Assert.Equal(storeData.AInt, dataReturned.AInt);
            Assert.Equal(storeData.ADateTime, dataReturned.ADateTime);
        }
    }

    public class TestStorage
    {
        public string? AString { get; set; }
        public int? AInt { get; set; }

        public DateTime? ADateTime { get; set; }
    }
}