using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.Model;
using Moq;
using NetDaemon.Model3.Internal;
using Xunit;

namespace NetDaemon.Model3.Tests.Internal
{
    public class EntityStateCacheTest
    {
        [Fact]
        public async void StateChangeEventIsFirstStoredInCacheThanForwarded()
        {
            // Arrange
            using var testSubject = new Subject<HassEvent>();
            var hassClientMock = new Mock<IHassClient>();
            hassClientMock.Setup(m => m.GetAllStates(CancellationToken.None)).ReturnsAsync(new List<HassState>());
                
            using var cache = new EntityStateCache(hassClientMock.Object, testSubject);
            await cache.InitializeAsync(CancellationToken.None);
            var handlerCalled = false;

            var changedEventData = new HassStateChangedEventData()
            {
                EntityId = "sensor.test",
                OldState = new HassState(),
                NewState = new HassState()
                {
                    State = "newState"
                        
                } 
            };
            
            cache.StateAllChanges.Subscribe(e =>
            {
                // Verify that in the event handler the cache is already updated
                Assert.Equal("newState", cache.GetState("sensor.test")?.State);
                Assert.Equal(changedEventData, e);
                handlerCalled = true;
            });

            // Act
            testSubject.OnNext(new HassEvent {
                Data = changedEventData 
            });
            
            // Assert
            Assert.True(handlerCalled);
        }
    }
}