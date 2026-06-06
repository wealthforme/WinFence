# WinFence

[中文](#中文) · [English](#english)

---

## 中文

**WinFence**(中文名:桌面网格整理)是一个真正**免费、开源、轻量**的 Windows 桌面图标整理工具,对标 Stardock Fences。

### 核心理念

- **永久免费 + 开源**(MIT 协议)—— 没有订阅、没有广告、没有捆绑、没有后门
- **轻量** —— 目标 < 30MB,冷启动 < 1 秒
- **网格化 + 自由** —— 围栏内部按网格 snap 对齐,围栏本身自由摆放 + 磁吸对齐
- **配置透明** —— JSON 可读,容易备份迁移
- **中文优先** —— 国内用户上手零成本

### 我们跟 Fences 有什么不同

| | Fences 6 | WinFence |
|---|---|---|
| 价格 | $8.99 起 + 订阅 | **永久免费** |
| 开源 | 否 | **MIT** |
| 网格化 | 自由拖动(无网格) | 内部 snap + 围栏磁吸 |
| 体积 | 越来越大 | < 30MB |
| 数据 | 私有 | 本地 JSON,可选 GitHub Gist 同步 |

### 路线图

详见 [PLAN.md](./PLAN.md)。预计 **8-9 周**到可上架版本。

- **M0** 基础设施(WPF 脚手架 + 透明窗口 + 托盘)
- **M1** WorkerW 集成 + z-order 控制(关键风险点)
- **M2** 围栏核心(创建/折叠/拖动/持久化)
- **M3** 体验与差异化(多屏/规则引擎/Chameleon/Peek)
- **M4** Steam 资产 + 打包上架

### 当前状态

🚧 **早期开发中**。当前在 M0。

### 开发

环境要求:
- Windows 10 1809+ 或 Windows 11
- .NET 8 SDK

```bash
git clone https://github.com/wealthforme/WinFence.git
cd WinFence
```

### License

[MIT](./LICENSE)

---

## English

**WinFence** is a truly **free, open-source, lightweight** Windows desktop icon organizer — an alternative to Stardock Fences.

### Why

- Fences 6 went subscription — many users want out
- NoFences (the only open-source alternative) is barely maintained
- There's a real gap for a "feature-complete + open-source + free" option

### Roadmap

See [PLAN.md](./PLAN.md). Estimated **8-9 weeks** to shippable MVP.

### Status

🚧 **Early development**. Currently in M0.

### Development

Requires:
- Windows 10 1809+ or Windows 11
- .NET 8 SDK

### License

[MIT](./LICENSE)
