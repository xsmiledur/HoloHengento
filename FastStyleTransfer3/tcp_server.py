#from __future__ import print_function
import socket
from contextlib import closing
import fast_style_change as fsc
import numpy as np

def main():
#  host = '127.0.0.1'
    host = '192.168.0.4'
    port = 4000
    backlog = 10
    bufsize = 4096

    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    with closing(sock):
        sock.bind((host, port))
        sock.listen(backlog)
        while True:
            conn, address = sock.accept()
            with closing(conn):
                data = conn.recv(bufsize)
                img = np.array(data, dtype=np.uint8)
                print(img)
                # img = np.ndarray(data, dtype=np.uint8, 3)
                output = fsc.stylize(img, 2, "aaa", False)
                conn.send(output)
    return

if __name__ == '__main__':
  main()