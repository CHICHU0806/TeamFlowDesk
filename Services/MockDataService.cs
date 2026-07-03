using TeamFlowDesk.Models;

namespace TeamFlowDesk.Services;

public static class MockDataService
{
    public static List<ProjectItem> GetProjects()
    {
        return new List<ProjectItem>
        {
            new()
            {
                Id = 1,
                Name = "TeamFlowDesk 团队工作流管理系统",
                Description = "面向团队负责人的内部私有化宏观管理软件。",
                OwnerName = "杨扬文琦",
                Status = "进行中",
                CurrentStage = "核心功能开发",
                RiskLevel = "正常",
                StartDate = new DateTimeOffset(2026, 6, 27, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2026, 7, 10, 0, 0, 0, TimeSpan.Zero),
                ProgressPercent = 35
            },
            new()
            {
                Id = 2,
                Name = "RoboMaster 队伍管理流程优化",
                Description = "围绕项目、任务、人员、器材和数据沉淀建立管理闭环。",
                OwnerName = "队长 / 项目经理",
                Status = "规划中",
                CurrentStage = "需求梳理",
                RiskLevel = "低风险",
                StartDate = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero),
                ProgressPercent = 15
            }
        };
    }

    public static List<TaskItem> GetTasks()
    {
        return new List<TaskItem>
        {
            new()
            {
                Id = 1,
                ProjectId = 1,
                Title = "完成 WinUI 项目基础框架",
                Description = "创建 NavigationView 项目，完成左侧导航栏与页面切换。",
                OwnerName = "杨扬文琦",
                Collaborators = "无",
                Status = "已完成",
                Priority = "高",
                RiskLevel = "正常",
                Deadline = new DateTimeOffset(2026, 6, 30, 0, 0, 0, TimeSpan.Zero),
                RelatedEquipment = "个人电脑",
                OutputRequirement = "可运行的 WinUI 页面骨架"
            },
            new()
            {
                Id = 2,
                ProjectId = 1,
                Title = "建立核心数据模型",
                Description = "建立项目、任务、成员、器材、AI记录和周报模型。",
                OwnerName = "杨扬文琦",
                Collaborators = "AI 协作",
                Status = "进行中",
                Priority = "高",
                RiskLevel = "正常",
                Deadline = new DateTimeOffset(2026, 7, 3, 0, 0, 0, TimeSpan.Zero),
                RelatedEquipment = "Rider / .NET SDK",
                OutputRequirement = "Models 文件夹中的核心数据类"
            },
            new()
            {
                Id = 3,
                ProjectId = 1,
                Title = "完善管理驾驶舱展示",
                Description = "展示项目进度、任务数量、成员负载、器材异常和 AI 协作记录。",
                OwnerName = "杨扬文琦",
                Collaborators = "无",
                Status = "待处理",
                Priority = "普通",
                RiskLevel = "正常",
                Deadline = new DateTimeOffset(2026, 7, 5, 0, 0, 0, TimeSpan.Zero),
                RelatedEquipment = "无",
                OutputRequirement = "可演示的管理驾驶舱页面"
            }
        };
    }

    public static List<MemberItem> GetMembers()
    {
        return new List<MemberItem>
        {
            new()
            {
                Id = 1,
                Name = "杨扬文琦",
                Grade = "人工智能 01 班",
                Direction = "项目管理 / 桌面开发 / AI 应用",
                Role = "项目负责人",
                SkillTags = "C#、WinUI、项目管理、文档整理、AI协作",
                AbilityLevel = "可独立完成",
                CurrentTaskCount = 3,
                WorkloadStatus = "正常",
                GrowthPlan = "继续完善 WinUI 页面开发、数据绑定和 SQLite 持久化能力。"
            },
            new()
            {
                Id = 2,
                Name = "示例成员 A",
                Grade = "大一",
                Direction = "电控方向",
                Role = "普通成员",
                SkillTags = "C/C++、STM32、CAN、PID",
                AbilityLevel = "熟悉",
                CurrentTaskCount = 2,
                WorkloadStatus = "正常",
                GrowthPlan = "后续可承担底盘控制相关任务。"
            }
        };
    }

    public static List<EquipmentItem> GetEquipment()
    {
        return new List<EquipmentItem>
        {
            new()
            {
                Id = 1,
                Name = "个人开发电脑",
                Code = "DEV-PC-001",
                Category = "开发设备",
                Status = "使用中",
                Location = "个人工作区",
                CurrentHolder = "杨扬文琦",
                RelatedTask = "WinUI 桌面应用开发",
                MaintenanceRecord = "状态正常"
            },
            new()
            {
                Id = 2,
                Name = "F407 主控板",
                Code = "CTRL-F407-001",
                Category = "电控器材",
                Status = "可用",
                Location = "实验室器材箱",
                CurrentHolder = "电控方向",
                RelatedTask = "底盘控制系统调试",
                MaintenanceRecord = "暂无异常"
            },
            new()
            {
                Id = 3,
                Name = "M3508 电机",
                Code = "MOTOR-3508-001",
                Category = "执行器",
                Status = "待检查",
                Location = "实验室器材箱",
                CurrentHolder = "底盘组",
                RelatedTask = "底盘运动测试",
                MaintenanceRecord = "需要检查线材连接状态"
            }
        };
    }

    public static List<AiRecordItem> GetAiRecords()
    {
        return new List<AiRecordItem>
        {
            new()
            {
                Id = 1,
                RelatedModule = "项目规划",
                Question = "两周开发周期内应该如何压缩系统功能范围？",
                AiSuggestion = "优先完成管理驾驶舱、任务管理、人员管理、器材管理和 AI 协作记录，暂缓多人协作和云同步。",
                HumanJudgement = "建议符合当前实训周期，功能范围需要控制在可演示 MVP 内。",
                FinalDecision = "保留核心模块，先完成单机版原型。",
                AdoptionStatus = "采纳",
                CreatedAt = DateTimeOffset.Now
            },
            new()
            {
                Id = 2,
                RelatedModule = "技术路线",
                Question = "是否使用 WPF 代替 WinUI？",
                AiSuggestion = "WPF 更稳定，但 WinUI 更符合 Microsoft 现代桌面生态。",
                HumanJudgement = "项目长期目标是基于 Microsoft 生态构建现代桌面应用，因此坚持使用 WinUI。",
                FinalDecision = "采用 C# + WinUI 3 + Windows App SDK。",
                AdoptionStatus = "部分采纳",
                CreatedAt = DateTimeOffset.Now
            }
        };
    }

    public static List<WeeklyReportItem> GetWeeklyReports()
    {
        return new List<WeeklyReportItem>
        {
            new()
            {
                Id = 1,
                Title = "软件综合实践第一周进度报告",
                StartDate = new DateTimeOffset(2026, 6, 27, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2026, 7, 3, 0, 0, 0, TimeSpan.Zero),
                CompletedWork = "完成项目选题、系统定位、技术路线确定、WinUI 项目创建、页面骨架搭建和 Git 仓库初始化。",
                Problems = "第一周暂未遇到重大技术问题，主要问题集中在 WinUI 模板创建和 Git 仓库权限配置方面，均已解决。",
                NextPlan = "下一步将完成核心数据模型、模拟数据服务、管理驾驶舱统计展示和基础页面数据绑定。",
                AiCollaborationSummary = "AI 主要参与了项目定位梳理、功能范围压缩、周报内容整理和开发步骤规划。",
                ManagerReview = "当前进度正常，系统框架已经搭建完成，后续需要重点完成数据展示和基础交互功能。",
                ProgressStatus = "正常"
            }
        };
    }
}