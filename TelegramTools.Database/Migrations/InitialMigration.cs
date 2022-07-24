using FluentMigrator;

namespace TelegramTools.Database.Migrations;

[Migration(202207160001)]
public class InitialMigration : Migration
{
    public override void Down()
    {
        Delete.Table("DocumentFiles");
        Delete.Table("DuplicateDocuments");
        Delete.Table("Statistics");
    }

    public override void Up()
    {
        Create.Table("Documents")
            .WithColumn("TelegramId").AsInt64().PrimaryKey().Identity().NotNullable().Indexed()
            .WithColumn("Name").AsString().NotNullable().Indexed().Unique()
            .WithColumn("Extension").AsString(10).NotNullable()
            .WithColumn("Path").AsString().NotNullable().Unique()
            .WithColumn("Hash").AsBinary().NotNullable().Unique().Indexed();
        Create.Table("Duplicates")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity().NotNullable()
            .WithColumn("OriginalName").AsString().NotNullable()
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Hash").AsBinary().NotNullable().Indexed()
            .WithColumn("TelegramId").AsInt64().NotNullable().Indexed().Unique();
    }
}