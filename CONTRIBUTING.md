# Contributing to GB Inventory System

Thank you for considering contributing to the project!. We welcome contributions of all kinds - from code to documentation and testing.

---

## 🧭 How to Contribute

1. **Fork** this repository and clone your fork locally.
2. Create a **new branch** for your feature or fix:
   ```bash
   git checkout -b feat/your-feature-name
   ```
3. Make your changes in Unity or Visual Studio.
4. Run all unit tests using Unity's Test Runner (`Window -> General -> Test Runner`)
5. Commit and push your changes.
```bash
git add .
git commit -m "feat: short description"
git push origin feat/your-feature-name
```
6. Open a Pull Request using the provided template.

---

## 🧪 Testing

All test scripts are located in:
```
Assets/InventorySystem/Tests/
```
Before submitting:
- Ensure all tests pass
- Avoid breacking existing tests
- Add new test cases when adding features

---

## 🧱 Code Style

- Use English for code, comments, and commit messages
- Use `PascalCase` for classes and `camelCase` for variables
- Document all public APIs with `/// <summary>` comments
- Keep domain logic free from Unity dependencies
- Keep systems data-driven and decoupled

---

## 📜 License
By contributing, you agree that your code will be released under the MIT License, the same license as the resto of the project.
