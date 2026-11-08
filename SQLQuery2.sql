SELECT
    t.TopicId,
    t.TopicName,
    v.VocabularyId,
    v.EnglishWord,
    v.VietnameseMeaning,
    v.Status
FROM dbo.Topics t
LEFT JOIN dbo.Vocabulary v
    ON t.TopicId = v.TopicId
WHERE t.TopicName = 'Food';