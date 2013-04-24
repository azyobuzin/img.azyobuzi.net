# -*- coding: utf-8 -*-

from resolvers import *

class PhotoShare(Resolver):
    @property
    def service_name(self):
        return "Big Canvas PhotoShare"
    
    @property
    def regex_str(self):
        return r"^http://(?:www\.)?bcphotoshare\.com/photos/\d+/(\d+)/?(?:\?.*)?$"
    
    #large.jpgとoriginal.jpgは同じ
    #拡張子は関係ない

    #重い。画像取得できないし、事実上閉鎖してる感じある。
    
    def get_full(self, match):
        return "http://images.bcphotoshare.com/storages/%s/large.jpg" % match.group(1)

    def get_full_https(self, match):
        return None
    
    def get_large(self, match):
        return "http://images.bcphotoshare.com/storages/%s/large.jpg" % match.group(1)

    def get_large_https(self, match):
        return None
    
    def get_thumb(self, match):
        return "http://images.bcphotoshare.com/storages/%s/thumb180.jpg" % match.group(1)

    def get_thumb_https(self, match):
        return None

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
