import time

import cv2
from datetime import datetime
import img_pre_proc
import call_module
import json
import os

# 初始化相機
cap = cv2.VideoCapture(0, cv2.CAP_DSHOW)
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 8000)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 6000)
cnt = 0
stop_flag = False  # 全局停止旗標


def update_json(key, value):
    file_path = r"D:\cnc_gui_s-master\cnc_gui\bin\Debug\config.json"

    # 檢查文件是否存在，如果不存在則創建一個空的 JSON 文件
    if not os.path.exists(file_path):
        with open(file_path, 'w') as file:
            json.dump({}, file)

    try:
        # 讀取現有的 JSON 數據
        with open(file_path, 'r') as file:
            data = json.load(file)
        print(f"更新鍵: {key}, 更新值: {value}")
        # 更新正確的 key
        data[key] = value  # roi1 積屑占比
        # 將更新的內容寫入到 JSON 文件
        with open(file_path, 'w') as file:
            json.dump(data, file, indent=4)

    except Exception as e:
        print(f"An error occurred: {e}")


def take_picture():
    if not cap.isOpened() or stop_flag:
        print("停止拍攝或無法抓取鏡頭")
        return

    ret, frame = cap.read()
    if ret:
        timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
        filename = f"photo_{timestamp}.jpg"

        # 開啟一個新執行緒來保存圖片
        # threading.Thread(target=cv2.imwrite, args=(filename, frame)).start()
        # threading.Thread(target=imgpro.proc_0919, args=(frame, timestamp)).start()
        # cv2.imwrite(filename, frame)
        r1, r2 = img_pre_proc.pre_proc(frame, timestamp)
        res1 = -87
        res2 = call_module.predict(r2, 'ROI 2')
        # res2 = call_module.predict_r2(r2, 'ROI 2')
        # print(res2)
        update_json('Flusher_level_bar_R1', int(res1))  # int64 to int
        update_json('Flusher_level_bar_R2', int(res2))

        global cnt
        cnt += 1
        print(f"已拍攝照片並保存為： {filename} [{cnt}]")
    else:
        print("無法讀取鏡頭 frame")


if __name__ == '__main__':
    time.sleep(2)
    take_picture()
