HAWKAIToolkit - Unity AI 工具包

HAWKAIToolkit 是一个用于在 Unity 中集成各种 AI 服务（如 OpenAI, Ollama）的框架。它提供了简单易用的 API，让你可以轻松地在游戏中实现聊天、流式输出和图像理解等功能。

目录

- [项目结构](#项目结构)
- [如何使用](#如何使用)
- [示例场景](#示例场景)
- [贡献](#贡献)



# 项目结构

以下是项目的目录结构，帮助你快速了解代码的组织方式：

*   **Core**: 工具包的核心逻辑，如接口定义、管理类和数据模型。
*   **Editor**: 创建 OpenAIProvider 的编辑器扩展。
*   **Providers**: 具体 AI 服务提供商的实现。
*   **Runtime**: 在游戏运行时需要的基础功能。
*   **Samples**: 示例场景，展示如何使用工具包。

# 快速入门

核心功能解释，帮助你快速入门：

1. 将 `AIToolkit` 文件夹导入到你的 Unity 项目中

2. 创建一个 Provider：`Assets - Create - AITookit - AIProvider`
![项目截图](./docs/01%20创建Provider.png)

3. 已创建的 AIProvider
![项目截图](./docs/02%20Provider%20介绍.png)
   - 选择厂商预设：预设了国内常用的AI服务商
      - 支持Qwen（百炼）、DeepSeek、Zhipu AI（智谱清言）、Moonshot（Kimi）
      - 基于 OpenAI 格式的都可选 OpenAI 接入
      - 基于 OpenAI 格式的都可选 OpenAI 接入
   - BaseURL：模型请求地址，如选预设，此项会自动更新
   - APIKey：模型服务商的APIKey（如有），点击 `申请 API Key` 自动跳转申请网页
   - Model：模型ID，即模型名称，需严格填入ID
   - Timeout：设置 UnityWebRequest 网络请求超时秒数

4. 使用 `AISimpleHelper`、`AIStreamHelper` 或 `AIVisionHelper` 来调用 AI 服务。

# 示例场景

提供的两个示例场景

1. `01 SimpleAIChat`：基础 AI 流式和非流式调用，含输出到 UI 和 Console 示例
2. `02 VisionAIChat`：图像理解 AI 流式和非流式调用，在基础示例上新增了包含 UGUI 运行示例

# 贡献

欢迎提交 Issue 和 Pull Request！
