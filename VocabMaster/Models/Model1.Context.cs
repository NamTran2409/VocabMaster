namespace VocabMaster.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class VocabMasterDBEntities : DbContext
    {
        public VocabMasterDBEntities()
            : base("name=VocabMasterDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ExternalLogin> ExternalLogins { get; set; }
        public virtual DbSet<QuestionOption> QuestionOptions { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; set; }
        public virtual DbSet<QuizAttempt> QuizAttempts { get; set; }
        public virtual DbSet<Quizze> Quizzes { get; set; }
        public virtual DbSet<Topic> Topics { get; set; }
        public virtual DbSet<UserDailyActivity> UserDailyActivities { get; set; }
        public virtual DbSet<UserLearningSetting> UserLearningSettings { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserVocabularyProgress> UserVocabularyProgresses { get; set; }
        public virtual DbSet<Vocabulary> Vocabularies { get; set; }
    }
}
