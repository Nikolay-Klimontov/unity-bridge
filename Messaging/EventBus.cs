using System;
using System.Collections.Generic;

namespace Game.Core.Messaging
{
    public class EventBus : IEventBus
    {
        #region INTERNAL_VARIABLES

        private Dictionary<Type, List<ISubscription>> subscriptions = new Dictionary<Type, List<ISubscription>>();

        private uint messageSendingDepth = 0;
        List<ISubscription> outdatedSubscriptions  = new List<ISubscription>();
        #endregion
        
        
        public SubscriptionToken Subscribe<TEvent>(Action<TEvent> action) where TEvent : BasicEvent
        {
            if (!subscriptions.ContainsKey(typeof(TEvent)))
            {
                subscriptions.Add(typeof(TEvent), new List<ISubscription>());
            }
            
            var sub = new Subscription<TEvent>(action, this);
            
            subscriptions[typeof(TEvent)].Add(sub);

            return sub.Token;
        }

        public void Unsubscribe(SubscriptionToken subscription)
        {
            if (subscriptions.ContainsKey(subscription.EventType))
            {
                var sub = subscriptions[subscription.EventType].Find(x => x.Token.UID == subscription.UID);
                if(sub == null)
                    return;
                
                sub.Invalidate();
                outdatedSubscriptions.Add(sub);
            }
        }

        public void Publish<TEvent>(TEvent e) where TEvent : BasicEvent
        {
            if (!subscriptions.ContainsKey(typeof(TEvent))) return;
            
            //Problem: If subscriber remove subscription in the handler for this event
            //         Count for list will be changed too, also it will shift tail to the left
            //         so actually Index will be pointing far ahead and subscribers will not receive events
            /*
                * - visited
                v - current indexer position
                x - removed element
                ? - skipped 
                                        v
                original: [*][*][*][*][ ][ ][ ][ ][ ]
                           1  2  3  4  5  6  7  8  9     
                removing: [x][ ][x][ ][x][ ][ ][ ][ ]
             after shift: [*][*][?][?][?][ ]                        
            original idx: [2][4][6][7][8][9]
              new indexr:                 ^
            */ 
            
            //TODO: need more intelligent way to resolve
            messageSendingDepth++; //kinda semaphore
            {
                var mailingList = subscriptions[typeof(TEvent)];
                //Problem: collection can be changed from outside 
                //         this will invalidate iterators in foreach loop 
                //Solution: moved to basic for loop, do not change that
                for (var i = 0; i < mailingList.Count; mailingList[i++].Publish(e))
                {
                }
            }
            messageSendingDepth--;
            
            // if all sending done then we can safely clear all outdated subscriptions
            if (messageSendingDepth == 0 && outdatedSubscriptions.Count > 0)
            {
                ClearSubscriptions();
            }
        }

        private void ClearSubscriptions()
        {
            foreach (var subscription in outdatedSubscriptions)
            {
                subscriptions[subscription.Token.EventType].Remove(subscription);
            }
            
            outdatedSubscriptions.Clear();
        }
    }
}