import cv2
import numpy as np
import os

current_dir = os.path.dirname(os.path.abspath(__file__))

ms1 = np.array([[0, 2], [980, 118], [708, 294], [608, 390], [460, 554], [356, 706],
                [300, 818], [220, 1010], [188, 1130], [168, 1234], [152, 1362]])

ms2 = np.array([[7, 38], [69, 207], [146, 376], [176, 438], [253, 546],
                [330, 653], [376, 730], [438, 807], [484, 869], [546, 930],
                [607, 992], [684, 1053], [761, 1130], [823, 1176], [884, 1223],
                [992, 1315], [1053, 1346], [1130, 1392], [1192, 1423],
                [1238, 1453], [1238, 1576], [1269, 1807], [1300, 2084],
                [1315, 2207], [1346, 2376], [1469, 3238], [346, 3469]])


def mask_roi(img, ps):
    mask_shape = img.shape[:2]
    mask = np.zeros(mask_shape, dtype=np.uint8)
    cv2.fillPoly(mask, [ps], 255)
    roi = cv2.bitwise_and(img, img, mask=mask)
    if len(roi.shape) > 2:
        roi = cv2.cvtColor(roi, cv2.COLOR_BGR2GRAY)
    return roi


def get_roi(image):
    # image = cv2.imread(r"C:\Users\natsumi\PycharmProjects\pythonProject\image\photo_20240718_153508.png")
    # x, y
    # [(75, 58), (575, 113), (561, 647), (138, 648)]
    # pts1 = np.array([[370, 410], [1342, 410], [1342, 1529], [370, 1529]], np.int32)
    # pts1 = np.array([[75, 58], [575, 113], [561, 647], [138, 648]], np.int32)
    # pts1 = np.array([(44, 33), (348, 68), (341, 430), (89, 443)])
    pts1 = np.array([[290, 463], [1323, 546], [
                    1273, 1940], [450, 1973]], np.int32)
    pts2 = np.array([[560, 2539], [1550, 2504], [
                    2053, 5699], [880, 5982]], np.int32)

    # 最小邊界矩形並擷取 ROI
    x1, y1, w1, h1 = cv2.boundingRect(pts1)
    roi1 = image[y1:y1 + h1, x1:x1 + w1]
    x2, y2, w2, h2 = cv2.boundingRect(pts2)
    roi2 = image[y2:y2 + h2, x2:x2 + w2]
    # cv2.imshow('roi1', cv2.resize(roi1, None, fx=0.2, fy=0.2))
    # cv2.imshow('roi2', cv2.resize(roi2, None, fx=0.2, fy=0.2))
    # cv2.waitKey()
    # cv2.destroyAllWindows()
    print(
        f'pts1 = ({x1}, {y1}), ({x1}, {y1 + h1}), ({x1 + w1}, {y1 + h1}), ({x1 + w1}, {y1})')
    print(
        f'pts2 = ({x2}, {y2}), ({x2}, {y2 + h2}), ({x2 + w2}, {y2 + h2}), ({x2 + w2}, {y2})')
    return roi1, roi2


def pre_proc(img, timestamp):
    global current_dir
    current_img = img
    # cv2.imshow('img', cv2.resize(current_img, None, fx=0.5, fy=0.5))
    roi1, roi2 = get_roi(current_img)  # 獲取兩個 ROI 區域

    # cv2.imshow('roi1', cv2.resize(roi1, None, fx=0.5, fy=0.5))
    # cv2.waitKey()
    # cv2.destroyAllWindows()

    # save origin image
    ori_dir = os.path.join(current_dir, 'origin')
    if not os.path.exists(ori_dir):
        os.makedirs(ori_dir)
    ori_filename = os.path.join(ori_dir, 'ori.jpg')
    cv2.imwrite(ori_filename, img)

    # save roi1 to r1 dir
    r1_dir = os.path.join(current_dir, 'r1')
    if not os.path.exists(r1_dir):
        os.makedirs(r1_dir)
    r1_filename = os.path.join(r1_dir, 'r1.jpg')
    cv2.imwrite(r1_filename, roi1)

    # save roi2 to r2 dir
    r2_dir = os.path.join(current_dir, 'r2')
    if not os.path.exists(r2_dir):
        os.makedirs(r2_dir)
    r2_filename = os.path.join(r2_dir, 'r2.jpg')
    cv2.imwrite(r2_filename, roi2)

    fx, fy = 0.1, 0.1

    # 灰階處理
    g1, g2 = roi1, roi2
    if len(roi1) == 3:
        g1 = cv2.cvtColor(roi1, cv2.COLOR_BGR2GRAY)
    if len(roi2) == 3:
        g2 = cv2.cvtColor(roi2, cv2.COLOR_BGR2GRAY)

    # 掩膜處理
    r1 = mask_roi(g1, ms1)
    r2 = mask_roi(g2, ms2)
    r1 = cv2.resize(r1, None, fx=fx, fy=fy)
    r2 = cv2.resize(r2, None, fx=fx, fy=fy)
    return r1, r2
