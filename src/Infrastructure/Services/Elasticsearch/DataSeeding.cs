using Domain.Aggregates.AuditLogs;
using Domain.Aggregates.AuditLogs.Enums;
using Domain.Aggregates.Users.Enums;
using Elastic.Clients.Elasticsearch;
using FluentConfiguration.Configurations;

namespace Infrastructure.Services.Elasticsearch;

public static class DataSeeding
{
    /// <summary>
    /// fake data for test
    /// </summary>
    /// <param name="elasticsearchClient"></param>
    /// <returns></returns>
    public static async Task SeedingAsync(
        ElasticsearchClient elasticsearchClient,
        string? prefix = null
    )
    {
        var auditLog = await elasticsearchClient.SearchAsync<AuditLog>();
        if (auditLog.Documents.Count > 0)
        {
            return;
        }

        List<AuditLog> auditLogs =
        [
            new AuditLog()
            {
                Entity =
                    "Theo Heatworld, Victoria Beckham đang rất lo lắng khi David Beckham dồn hết tâm huyết và thời gian cho điền trang Cotswolds. Như Heatworld đưa tin trước đó, cặp đôi này đã cân nhắc chuyển đến sống ở khu điền trang trị giá 12 triệu bảng Anh của họ ở Oxfordshire suốt một thời gian dài. David gần đây chia sẻ trên Instagram khung cảnh của điền trang cũng như việc làm khoai tây chiên giòn do anh tự trồng và thu thập trứng gà.",
                Type = (int)AuditLogType.Create,
                ActionPerformBy = "01J8HF8PE8SYB0NRVCC8CZGA11",
                Agent = new Agent()
                {
                    FirstName = "Sáng",
                    LastName = "Trần Minh Sáng",
                    Email = "anna.kim71@gmail.com",
                    DayOfBirth = DateTime.Parse("1990-07-08T00:00:00"),
                    Gender = (int)Gender.Other,
                },
            },
            new AuditLog()
            {
                Entity =
                    "Trong buổi phỏng vấn vào tháng 7, Karim Benzema, đồng đội cũ của Vinicius tại Real Madrid, cũng dự đoán chiến thắng thuộc về chân sút sinh năm 2000. 'Chủ nhân Quả bóng vàng sao? Tôi sẽ chọn Vinicius, đơn giản là vì cậu ấy xứng đáng. Không chỉ năm nay, mà năm ngoái cậu ấy cũng chơi rất hay. Vini vượt trội so với các cầu thủ khác ở kỹ năng cá nhân. Cậu ấy đã là một cầu thủ rất toàn diện'.",
                Type = (int)AuditLogType.Create,
                ActionPerformBy = "01J8HF8PE8SYB0NRVCC8CZGA11",
                Agent = new Agent()
                {
                    FirstName = "Tiên",
                    LastName = "Nguyễn",
                    Email = "anna.kim71@gmail.com",
                    DayOfBirth = DateTime.Parse("1990-07-09T00:00:00"),
                    Gender = (int)Gender.Female,
                },
            },
            new AuditLog()
            {
                Entity =
                    "Trong số nhiều trang bị bôi đen, tài liệu pháp lý bao gồm những cáo buộc cho rằng các thí sinh phải chịu đựng môi trường làm việc 'dung dưỡng văn hóa kỳ thị giới và phân biệt đối xử với phụ nữ'. Những cáo buộc này đánh vỡ hình ảnh của MrBeast, thường được xem là một trong những người tử tế nhất trên Internet.",
                Type = (int)AuditLogType.Update,
                ActionPerformBy = "01J8HHP6VGVDGWSP08KG82AFCD",
                Agent = new Agent()
                {
                    FirstName = "Hiếu",
                    LastName = "Trần Minh Hiếu",
                    Email = "anna.kim71@gmail.com",
                    DayOfBirth = DateTime.Parse("1990-07-10T00:00:00"),
                    Gender = (int)Gender.Male,
                },
            },
        ];

        var response = await elasticsearchClient.IndexManyAsync(
            auditLogs,
            ElsIndexExtension.GetName<AuditLog>(prefix)
        );

        if (response.IsSuccess())
        {
            Console.WriteLine("Elasticsearch has seeded.");
        }
        else
        {
            Console.WriteLine(
                $"Elasticsearch has been failed in seeding with {response.DebugInformation}"
            );
        }
    }
}
