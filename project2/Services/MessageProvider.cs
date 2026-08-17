namespace project2.Services;

public class MessageProvider
{
    private static readonly string[] Messages =
    {
        "Ты супер! 🌸",
        "У тебя всё получается! 💫",
        "Ты светишься ❤️",
        "Ты невероятная! ✨",
        "Лучшая версия себя ❤️",
        "Ты делаешь этот мир лучше ☀️",
        "Я восхищаюсь тобой 💖",
        "Ты разбила эту задачу! 🔥",
        "Сложности — твоя стихия! 💪",
        "Ты преодолела это красиво ✨",
        "Эта победа только твоя! 🏆",
        "Ты улыбка моего дня ❤️",
        "С тобой даже понедельник — праздник ☀️",
        "Ты мой источник вдохновения ✨",
        "Красота, как всегда на высоте! 💖",
        "Ты справишься, я знаю ❤️",
        "Верю в тебя каждую секунду ✨",
        "Ты намного сильнее, чем думаешь 💪",
        "Всё будет хорошо, ты же моя героиня 🌟",
        "Ты — лучшее, что есть ❤️",
        "Спасибо, что ты есть ✨",
        "Ты — моя гордость 💖",
        "Бесконечно тобой восхищаюсь ❤️",
        "Ну просто секас 🔥"
    };

    private readonly List<string> _unused = new(Messages);
    private readonly Random _rng;

    public MessageProvider(Random rng)
    {
        _rng = rng;
    }

    /// Возвращает 1 сообщение, пока есть неиспользованные,
    /// после — по 2 случайных (с возможными повторами).
    public List<string> GetNext()
    {
        if (_unused.Count > 0)
        {
            var index = _rng.Next(_unused.Count);
            var message = _unused[index];
            _unused.RemoveAt(index);
            return new List<string> { message };
        }

        return new List<string>
        {
            Messages[_rng.Next(Messages.Length)],
            Messages[_rng.Next(Messages.Length)]
        };
    }

    private static readonly string[] ConsolingMessages =
    {
        "Ничего страшного, попробуй ещё раз! 💪",
        "Бывает, не переживай ❤️",
        "Ты справишься, я знаю! ✨",
        "Ещё одна попытка — и всё получится 🌟",
        "Не сдавайся! ❤️",
        "Ты можешь длашь, больше, комон эврибади!!!",
        "П - поддержка)"
    };

    public string GetConsolingMessage()
    {
        return ConsolingMessages[_rng.Next(ConsolingMessages.Length)];
    }
}