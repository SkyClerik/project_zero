- [ ] При добавлении предмета в стак не срабатывает событие добавления в инвентарь потому что не создается предмет

#### GlobalBox.cs

* Пространство имён (Namespace): SkyClerik.Utils
* Описание: Это наш главный менеджер! Он управляет состояниями игры, а ещё сервисами сохранения и загрузки. 
* GlobalBox регистрируется в **ServiceProvider** автоматически (в Awake() и отменяет регистрацию в OnDestroy()).

Чтобы получить доступ к его сервисам или свойствам, нужно просто взять его из ServiceProvider так:
```
GlobalBox globalBox = ServiceProvider.Get<GlobalBox>();
```

Далее получаем желаемые сервисы через свойства:

```cs
// Даст доступ к сервису сохранения данных.
SaveService SaveService { get; }
// Даст доступ к сервису загрузки данных.
LoadService LoadService { get; }
```
    
```cs
// Поможет узнать текущие глобальные состояние игры, глобальные флаги, настройки сохранения.
GlobalGameProperty GlobalGameProperty { get; }

//[Header("Состояние игры")]

[Tooltip("Текущее состояние игры (например, в главном меню, в игре, на паузе).")]
public GameState CurrentGameState => _currentGameState;

[Tooltip("Установлено в 'true', если это новая игра. 'false' - если загруженная.")]
public bool IsNewGame => _isNewGame;

//[Header("Глобальные флаги")]
 
[Tooltip("Установлено в 'true', если игрок уже посмотрел вступительный ролик/обучение.")]
public bool HasSeenIntro => _hasSeenIntro;

[Tooltip("Установлено в 'true', если игрок не хочет видеть автоматические подсказки.")]
public bool HasInfomatron => _hasInfomatron;

[Tooltip("Текущий счет или очки игрока.")]
public int PlayerScore => _playerScore;
 
[Tooltip("Установлено в 'true', если игроку разрешено открыть окно крафта")]
public bool MakeCraftAccessible { get => _craftAccessible; set => _craftAccessible = value; }

//[Header("Сохранение")]
 
[Tooltip("Индекс текущего слота сохранения (например, 0, 1, 2).")]
public int CurrentSaveSlotIndex => _currentSaveSlotIndex;
```
 
---

#### GlobalItemStorage.cs
 
* Пространство имён (Namespace): SkyClerik.Inventory
* Описание: Это наше глобальное хранилище для всех предметов и их префабов в игре. Тут можно найти все определения предметов и их визуальное представление.
* **GlobalItemStorage** регистрируется в **ServiceProvider** автоматически (в Awake() и отменяет регистрацию в OnDestroy()).

Чтобы получить доступ к его сервисам или свойствам, нужно просто взять его из ServiceProvider так:

```cs
GlobalItemStorage itemStorage = ServiceProvider.Get<GlobalItemStorage>();
```

Далее получаем желаемое хранилище через свойства:

```cs
// Даёт определение глобального хранилища всех данных предметов.
ItemsDataStorageDefinition GlobalItemsStorageDefinition { get; }

/// Возвращает клонированный ItemBaseDefinition по его itemID = (индексу в списке).
public ItemBaseDefinition GetClonedItem(int itemID)

/// Возвращает ItemBaseDefinition из внутреннего списка _baseDefinitions по указанному itemID = (индексу в списке).
/// Этот метод предоставляет прямой доступ к оригинальному объекту ItemBaseDefinition из списка,
/// что может быть полезно для чтения данных или для операций, не требующих клонирования.
/// Возвращает null, если индекс находится вне диапазона.
public ItemBaseDefinition GetOriginalItem(int itemID)
```

```cs
// Даёт определение хранилища префабов предметов.
ItemPrefabsStorageDefinition ItemPrefabsStorageDefinition { get; }

//Возвращает префаб для мира по его itemID (int). itemID должен совпадать с индексом в списке.
public GameObject GetPrefab(int itemID)
```

---

#### InventoryAPI.cs
  
* Пространство имён (Namespace): SkyClerik.Inventory
* Описание: Фасад для управления UI инвентаря. Предоставляет упрощенный доступ к основным функциям взаимодействия с инвентарем, крафтом, сундуком, лутом и экипировкой.
* **InventoryAPI** регистрируется в **ServiceProvider** автоматически (в Awake() и отменяет регистрацию в OnDestroy()).

Чтобы получить доступ к его сервисам или свойствам, нужно просто взять его из ServiceProvider так:

```cs
InventoryAPI inventoryAPI = ServiceProvider.Get<InventoryAPI>();
```

```cs
// Вызывается когда игрок нажал на предмет в инвентаре
internal event Action<ItemBaseDefinition> OnItemGiven;

// Вызывается, когда предмет успешно добавлен в инвентарь игрока.
public event Action<ItemBaseDefinition> OnPlayerItemAdded;

// Вызывается, когда предмет удален из инвентаря игрока.
public event Action<ItemBaseDefinition> OnPlayerItemRemoved;

// Вызывается при неудачной попытке добавить предмет (например, нет места).
public event Action<ItemBaseDefinition> OnPlayerAddItemFailed;

// Вызывается, когда предмет не найден.
public event Action<int, Type> OnItemFindFall;

// Вызывается, когда предмет подняли.
public event Action<ItemVisual, GridPageElementBase> OnItemPickUp;

// Вызывается, когда предмет положили.
public event Action<ItemVisual, GridPageElementBase> OnItemDrop;

// Назначить свойства для обводки требуемого предмета (цвет и ширина)
public void SetGivinItemTracinColor(Color newColor, int width)

// Указывает, виден ли наш инвентарь.
public bool IsInventoryVisible { get; set; }

// Указывает, видна ли страница крафта.
public bool IsCraftVisible { get; set; }

// Указывает, видна ли страница сундука.
public bool IsCheastVisible { get; set; }

// Откроет инвентарь, чтобы выбрать предмет по его ID. Если предмета нет, инвентарь не откроется. tracing true если предмет должен подсвечиваться в инвентаре
public void OpenInventoryFromGiveItem(int itemID, bool tracing);

// Откроет инвентарь для выбора конкретного предмета. Если ссылка на предмет пустая, инвентарь не откроется. tracing true если предмет должен подсвечиваться в инвентаре
public void OpenInventoryGiveItem(ItemBaseDefinition item, bool tracing);

// Открывает обычный инвентарь и страницу крафта.
public void OpenInventoryAndCraft();

// Открывает обычный инвентарь и страницу экипировки.
public void OpenInventoryAndEquip();

// Открывает обычный режим отображения инвентаря.
public void OpenInventoryNormal();

// Открывает страничку сундука.
public void OpenCheast();

// Закрывает вообще все странички UI инвентаря.
public void CloseAll();

// Пытается добавить предмет в инвентарь
public bool TryAddItemsToPlayerInventory(int itemID, out ItemBaseDefinition itemBaseDefinition)

// Прямое добавление в инвентарь игрока листа из ItemsList (Клоны создаются сами).
public void AddItemsToPlayerInventory(ItemsList itemsList)

// Пытается удалить предмет из инвентаря в указанном количестве в стаке
public ItemContainer.RemoveResult TryRemoveItemInPlayerInventory(int itemId, int count)
```

Пример для запроса предмета через itemID с возвратом по событию.

```cs
private void _bInventoryGive_clicked()
{
	if (_inventoryAPI.IsInventoryVisible)
	{
		_inventoryAPI.OnItemGiven -= OnItemGivenCallback;
		_inventoryAPI.CloseAll();
	}
	else
	{
		// открыть инвентарь для выбора предмета (отписки обязательные)
		_inventoryAPI.OnItemGiven += OnItemGivenCallback;
		_inventoryAPI.OpenInventoryFromGiveItem(itemID: 0);
	}
}

private void OnItemGivenCallback(ItemBaseDefinition itemBaseDefinition)
{
	if (itemBaseDefinition.ID == 0)
	{
		_inventoryAPI.OnItemGiven -= OnItemGivenCallback;
		Debug.Log($"выбран нужный предмет : {itemBaseDefinition.ID} - {itemBaseDefinition}");
		_inventoryAPI.CloseAll();
	}
}
```


#### LutContainer.cs : MonoBehaviour
* Пространство имён (Namespace): SkyClerik.Inventory
* Описание: Объект, представляющий собой контейнер для лута. Хранит список предметов, которые могут быть переданы в другой контейнер  в автоматическом режиме или через окно выбора игроком.
* Указывается как ссылка в сцене и имеет два режима работы

```cs
// ВАРИАНТ 1 - передача лута из контейнера в инвентарь игрока через поиск свободного места от большего предмета к меньшему
[SerializeField]
private LutContainer _developLut;
 
 _developLut.TransferItemsToPlayerInventoryContainer();
```

```cs
// ВАРИАНТ 2 - Открыть окно с лутом для предоставления игроку выбора (в разработке)
[SerializeField]
private LutContainer _developLut;
 
 _developLut.OpenLutPage();
```

#### LutContainerWrappe.cs
* Пространство имён (Namespace): SkyClerik.Inventory
* Описание: Объект, представляющий собой контейнер для лута. СОЗДАЕТ список предметов по переданному в конструктор списку itemID, которые могут быть переданы в другой контейнер в автоматическом режиме или через окно выбора игроком.

```cs
 public LutContainerWrapper(List<int> wrapperItemIndexes)
// ВАРИАНТ 1 - передача лута из контейнера в инвентарь игрока через поиск свободного места от большего предмета к меньшему
[SerializeField]
private LutContainer _developLut;
 
 _developLut.TransferItemsToPlayerInventoryContainer();
```

```cs
// ВАРИАНТ 2 - Открыть окно с лутом для предоставления игроку выбора (в разработке)
[SerializeField]
private LutContainer _developLut;
 
 _developLut.OpenLutPage();
```
