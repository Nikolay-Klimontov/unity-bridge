﻿# Stateful 

Набор классов для создания подсистем/сервисов, логика работы которых меняется в зависимости от их текущего состояния (например [GameLoop](../GameLoop/GameStateMachine.cs))

Работает на тех же [событиях]() что и [MessageSystem](../Messaging/Paper.Core.Messaging.md)
Текущему состоянию будут переданы все события, если они не активируют переход в другое состояние.
 
> **Не кидайте вызывающие переход события из OnEnter() и OnExit(), такой возможности пока нет**


## [IState](./IState.cs)

Общий интерфейс для всех состояний. 

Содержит прототипы методов: 

1. `OnEnter()` - вызывается при входе в состояние
2. `OnEvent<TEvent>(TEvent e) where TEvent : BasicEvent` - сюда перенаправляются все события от родительской машины
3. `OnLeave()` - вызывается при выходе из состояния
4. `HandleTransitionEvents` - ретранслировать ли событие, вызвавшее переход в/из этого состояние

> **Не оставляйте ссылки на стейты снаружи FSM**
### Иногда может быть полезным знать чем был вызван переход и как-то отреагировать на событие...

Состоянию при переходе в/из него может быть ретранслировано событие, вызвашее переход.

Для обработки таких событий в состояние должно выставить соответсвующую политику в поле `HandleTransitionEvents`.

> Событие будет послано в `OnEvent()`

Для состояния может быть выставлена одна из следующих политик:
1. `HandleEvents.No` - не требует пересылки события, вызвашие переход в/из текущего сотояния
2. `HandleEvents.Enter` - требует пересылки события, вызвавшего переход в текущее состояние
3. `HandleEvents.Exit` - требует пересылки события, вызвавшего переход из текущего состояния
4. `HandleEvents.Both` - требует пересылки событий, вызвавшего переход в/из текущего состояния

Порядок вызовов функций при переходе: 
`from.OnEvent(event) → from.OnExit() → to.OnEnter() → to.OnEvent(event)`


## [StateToken](./StateToken.cs)

Уникальный идентификатор состояния, выдаётся машиной при добавлении состояния в неё. Нужен при конфигурации переходов. 

> Возможность удаления состояний из машины пока не реализована

## [StateMachine](./StateMachine.cs)

Корневой конечный автомат и общая точка для создания своих конечных автоматов.

Унифицирован по Event с MessageSystem, может быть подключён к шине событий напрямую. Оперирует типами событий, а не конкретными инстансами.

> **Подписывайте корневую машину на все события, которые вы хотите получать из шины**

> **Старайтесь не подписывать внутренние состония на шину, иначе это может привести к выполнению кода в неактивных состояниях**

Предоставляет базовую реализацию конечного автомата. Если на событие зарегистрирован переход, машина его выполнит, иначе событие будет передано в текущее активное состояние/подчинённую машину.

Для работы требует хотя бы одного зарегистрированного состояния и выставленного StartState

### Общий порядок создания и настройки конечного автомата

#### 1. Регистрация состояний в машине

Создайте класс с интерфейсом `IState` и зарегистрируйте его инстанс в машине (сохраните токен для дальнейшей настройки переходов) вызвав `AddState()`

> **Не добавляйте один и тот же инстанс в разные машины. Пока что механизм отслеживания владения не реализован.** 


```
//Example.cs

    class Login : IState 
    {
        public void OnEnter()
        {
            // Sending event to systems that we want to validate account info
            MessageSystem.Bus.Publish(new StartLoginSequience(this)); 
        }

        public void OnEvent<TEvent>(TEvent e) where TEvent : BasicEvent
        {
            // Map to specific handlers if need
            switch (e)
           {
               case NoConnection @event:
                   Parent.Push(new LoginFailed(this));
                   break;
               case ConnectionLost @event:
                   Parent.Push(new Reconnect(this));
                   break;
               case AccountServerNotResponding @event:
               case InvalidAccountData @event:
                   Parent.Push(new AuthFailed(this));
                   break;
               
               default: return;
           }
        }
        
        public void OnExit()
        {
            // sequence complete, if there was an error or success
            MessageSystem.Bus.Publish(new FinishLoginSequience(this));
        }

        // other methods and handlers
        ...
        // methods end 
    }

    ...
    public StateMachine()
    {
        //Registering state in constructor

        var loginStateTok = AddState(new Login());
    }
```

#### 2. Добавьте переходы

Добавление переходов осуществляется вызовом `AddTransition<EventType>(from_token, to_token)` или при помощи хэлпера `Configure(token).AddTransition<EventType>(to_token)`. В случае использования хэлпера добавления можно вызывать по цепочке.

> **Переходы можно добавить только между двумя зарегистрированными в машине состояниями**

Пример:
```
    {
        ...
        var login = AddState(new Login());
        var hub = AddState(new Hub());
        ...
        //Adding transition between states directly
        AddTransition<LoginSuccessful>(login, hub);
        
        //Using helper to chain calls
        Configure(loginStateToken)
            .AddTransition<LoginFailed>(relogin)
            .AddTransition<ConnectionFailed>(relogin)
            .AddTransition<NoConnection>(error);
        ...
    }
```

#### 2.5 Подпишитесь на события [Опционально]

Если автомат работает на общей [шине](../Messaging/Paper.Core.Messaging.md), то было бы неплохо его на эту шину подписать. Подписаться на получение можно в любой момент

```
    public SampleStateMachine()
    {
        ...
        MessageSystem.Bus.Subscribe<SampleEventType>(Push)
    }
```

#### 3. Выставите стартовое состояние 

Машина изначально не находится ни в каком состоянии и не может обрабатывать события и переходы. 
необходимо выставить `StartState` у машины в одно из зарегистрированных ранее сосотояний. Для активации машины вызовите `Start()`

> Если `StartState` не выставлен, машина не сможет начать работу. Вызов `Start()` ни к чему не приведёт
> Машина не кидает исключений, проверяйте всё внимательно 

```
    //StateMachine.cs
    {
        ...
        StartState = intro;
    }

    //Init.cs
    {
        ...
        gameCore = new GameCoreStateMachine()
        //This can be called anywhere else
        gameCore.Start()
    }
```
После вызова `Start()` автомат сделат стартовое состояние активным с соответствующим вызовом `OnEnter()`

### Если надо что-то прекратить...

#### `Stop()`
У машины есть метод `Stop()`, который переведёт машину в неактивное состояние с вызовом `OnExit()` у текущего состояния.
В таком случае при вызове `Start()` машина начнёт работать с последнего состояния в котором она находилась до остановки (с вызовом `OnEnter()`).
 
#### `Reset()`
Если нужно привести машину в исходное состояние - вызывайте `Reset()`. 
> *Сброс состояния машины на стартовое доступен только при остановленной машине*

Это вернёт машину в исходное положение. Снова вызовите `Start()` для начала обработки событий.

## [SubMachine](./SubMachine.cs)

Готовый класс для создания вложенных конечных автоматов. По сути это `StateMachine` с примешанной реализацией интерфейса `IState`
 
По принципу создания и настройки ничем не отличается от `StateMachine`.

Добавление в родительскую `StateMachine` происходит той же функцией `AddState()`
```
var submachine = fsm.AddState(new MySubmachine()); 
```

> Желательно сконфигурировать починённую машину до добавления в родительскую 
> или сохранить ссылку на инстанс для дальнейшего конфигурирования

> Лучшее место для настройки машины - *конструктор*. 

> **Убедитесь что выставили `StartState`** иначе вложенная машина не будет активирована
 
### На всякий случай
При переходе родителя в данное состояние вложеннная машина будет активирована вызовом `OnEnter()`. 

События от вышестоящей машины будут перенаправлены из `OnEvent()` в `Push()`

При выходе по `OnLeave()` машина будет остановлена и сброшена к изначальному состоянию.
```
    //from SubMachine.cs

    ...
    public void OnEnter()
    {
        Start(); //Activate fsm
    }

    public void OnEvent<TEvent>(TEvent e) where TEvent : BasicEvent
    {
        Push(e); // Handle event
    }

    public void OnLeave()
    {
        Stop(); //Deactivate fsm
        Reset(); //Restore start state
    }
``` 


