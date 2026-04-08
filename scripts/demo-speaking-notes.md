# Impact Analysis Demo Notes

## 1) What This Module Does
- Detects whether a DOWN node is a true root failure.
- If it is a root, marks downstream nodes as impacted.
- If it is not a root, avoids false root-cause marking.
- Clears root cause and impact rows when recovery is triggered.

## 2) Demo Setup Commands
Run from project root:

```bash
make rebuild
make seed
```

## 3) Live Demo Flow

### Step A: Show API is up
```bash
curl http://localhost:5289/weatherforecast
```
Expected: HTTP response with JSON array.

### Step B: Root failure scenario
```bash
curl -X POST http://localhost:5289/api/impact-analysis/analyze/1
curl http://localhost:5289/api/impact-analysis/result/1
```
Say: Device 1 is treated as root, and downstream devices are marked impacted.

### Step C: Non-root scenario
```bash
curl -X POST http://localhost:5289/api/impact-analysis/analyze/3
curl http://localhost:5289/api/impact-analysis/result/3
```
Say: Device 3 is not root because its parent path is already failed/impacted.

### Step D: Recovery scenario
```bash
curl -X POST http://localhost:5289/api/impact-analysis/clear/1
curl http://localhost:5289/api/impact-analysis/result/1
```
Say: Root-cause and impacted rows are cleaned after clear.

## 4) One-Command Validation
```bash
make demo
```
Expected: all PASS lines and final success message.

## 5) Technical Highlights
- Root detection checks parent state before deciding root vs non-root.
- Downstream traversal is loop-safe using visited-node tracking.
- Cleanup logic removes stale root-cause/impact records.
