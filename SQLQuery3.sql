USE VocabMasterDB;
GO

-- Xóa điều kiện cũ
ALTER TABLE dbo.Quizzes
DROP CONSTRAINT CK_Quizzes_QuestionCount;
GO

-- Tạo điều kiện mới: cho phép QuestionCount = 0
ALTER TABLE dbo.Quizzes
ADD CONSTRAINT CK_Quizzes_QuestionCount
CHECK (QuestionCount >= 0);
GO