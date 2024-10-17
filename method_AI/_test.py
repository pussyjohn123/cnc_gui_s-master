import time
import cv2
from datetime import datetime
import img_pre_proc
import call_module
import json
import os

def update_json(key, value):
    file_path = r'config.json'

    # 檢查文件是否存在，如果不存在則創建一個空的 JSON 文件
    if not os.path.exists(file_path):
        with open(file_path, 'w') as file:
            json.dump({}, file)

    try:
        # 讀取現有的 JSON 數據
        with open(file_path, 'r') as file:
            data = json.load(file)

        # 更新正確的 key
        data[key] = value  # roi1 積屑占比
        # 將更新的內容寫入到 JSON 文件
        with open(file_path, 'w') as file:
            json.dump(data, file, indent=4)

    except Exception as e:
        print(f"An error occurred: {e}")


def take_picture():
    timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
    filename = f"photo_{timestamp}.jpg"

    # 開啟一個新執行緒來保存圖片
    # threading.Thread(target=cv2.imwrite, args=(filename, frame)).start()
    # threading.Thread(target=imgpro.proc_0919, args=(frame, timestamp)).start()
    frame = cv2.imread('photo_20240919_163006.png')
    # cv2.imwrite(filename, frame)
    r1, r2 = img_pre_proc.pre_proc(frame, timestamp)
    # res1 = -1
    # res2 = call_module.predict(r2, 'ROI 2')
    # # res2 = call_module.predict_r2(r2, 'ROI 2')
    # # print(int(res2))
    # update_json('Flusher_level_bar_r1', int(res1))
    # update_json('Flusher_level_bar_r2', int(res2))


if __name__ == '__main__':
    current_path = os.path.dirname(os.path.abspath(__file__))
    os.chdir(current_path)
    print(os.getcwd())
    # time.sleep(3)
    take_picture()
