namespace HomeMediaRemote.Status.Windows
{
	internal class GridLayout
	{
		// Список сохраняет порядок добавления (важно для корректного заполнения "дыр")
		private readonly List<Tile> _tiles = new();
		// Словарь для быстрого поиска по ключу
		private readonly Dictionary<string, Tile> _tileMap = new();

		private void RecalculateLayout()
		{
			var currentX = Bounds.X;
			var currentY = Bounds.Y;
			var rowMaxHeight = 0;
			var boundsRight = Bounds.Right;

			foreach (var tile in _tiles)
			{
				// Если текущий тайл не помещается в строку (и это не начало строки), переходим на следующую
				if (currentX > Bounds.X && currentX + tile.Size.Width > boundsRight)
				{
					currentX = Bounds.X;
					currentY += rowMaxHeight + Spacing.Y;
					rowMaxHeight = 0;
				}

				tile.Location = new Point(currentX, currentY);

				// Сдвигаем курсор по X для следующего элемента
				currentX += tile.Size.Width + Spacing.X;

				// Отслеживаем максимальную высоту в текущей строке для корректного переноса
				if (tile.Size.Height > rowMaxHeight)
				{
					rowMaxHeight = tile.Size.Height;
				}

				// Уведомляем подписчика о новых координатах
				tile.Callback?.Invoke(tile.Location);
			}
		}

		public Rectangle Bounds { get; }
		public Point Spacing { get; }

		public GridLayout(Rectangle bounds, Point spacing)
		{
			Bounds = bounds;
			Spacing = spacing;
		}

		public void AddTile(string key, Size size, Action<Point> callback)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				throw new ArgumentException("Ключ не может быть пустым.", nameof(key));
			}

			if (_tileMap.ContainsKey(key))
			{
				throw new ArgumentException("Тайл с таким ключом уже существует.", nameof(key));
			}

			var tile = new Tile
			{
				Key = key,
				Size = size,
				Callback = callback,
				Location = Point.Empty,
			};

			_tiles.Add(tile);
			_tileMap[key] = tile;

			// Пересчёт позиций всех элементов
			RecalculateLayout();
		}

		public bool RemoveTile(string key)
		{
			if (!_tileMap.TryGetValue(key, out var tile))
			{
				return false;
			}

			_tiles.Remove(tile);
			_tileMap.Remove(key);

			// После удаления список "схлопывается", и RecalculateLayout автоматически 
			// сдвинет все последующие элементы, закрывая "дыру"
			RecalculateLayout();
			return true;
		}

		public Point GetNextTileLocation(Size size)
		{
			var currentX = Bounds.X;
			var currentY = Bounds.Y;
			var rowMaxHeight = 0;
			var boundsRight = Bounds.Right;

			// Симулируем текущее состояние без модификации списков
			foreach (var tile in _tiles)
			{
				if (currentX > Bounds.X && currentX + tile.Size.Width > boundsRight)
				{
					currentX = Bounds.X;
					currentY += rowMaxHeight + Spacing.Y;
					rowMaxHeight = 0;
				}

				currentX += tile.Size.Width + Spacing.X;
				if (tile.Size.Height > rowMaxHeight)
				{
					rowMaxHeight = tile.Size.Height;
				}
			}

			// Проверяем, поместится ли будущий тайл в текущую строку
			if (currentX > Bounds.X && currentX + size.Width > boundsRight)
			{
				currentX = Bounds.X;
				currentY += rowMaxHeight + Spacing.Y;
			}

			return new Point(currentX, currentY);
		}

		private record Tile
		{
			public required string Key { get; init; }
			public required Size Size { get; init; }
			public required Action<Point> Callback { get; init; }
			public required Point Location { get; set; }
		}
	}
}
