const fs = require("fs");
const path = require("path");

const buildDir = path.join(__dirname, "Build");
const indexPath = path.join(__dirname, "index.html");

// 1. 將所有 .gz 檔案重新命名，移除 .gz
fs.readdirSync(buildDir).forEach(file => {
  if (file.endsWith(".gz")) {
    const oldPath = path.join(buildDir, file);
    const newPath = path.join(buildDir, file.replace(/\.gz$/, ""));
    fs.renameSync(oldPath, newPath);
    console.log(`Renamed: ${file} -> ${path.basename(newPath)}`);
  }
});

// 2. 更新 index.html 中的引用
let html = fs.readFileSync(indexPath, "utf8");
html = html.replace(/\.gz/g, ""); // 移除所有 .gz 的引用
fs.writeFileSync(indexPath, html);
console.log("Updated index.html ✅");
