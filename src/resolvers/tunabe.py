# -*- coding: utf-8 -*-

import urllib2

from resolvers import *

class Tunabe(StoringResolver):
    @property
    def service_name(self):
        return u"つなビィ"
    
    @property
    def regex_str(self):
        return r"^http://(?:[\w\-]+\.tuna\.be/(\d+)\.html|(?:www\.)?tuna\.be/(?:t|show/\w+)/([\w\-]+)/?)(?:\?.*)?$"
    
    def get_parameters(self, match):
        id = match.group(1)
        return id if id else match.group(2)
    
    def _work(self, param, cursor):
        table = "tunabe"
        columns = ["id", "org"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return result[0]
        
        try:
            response = urllib2.urlopen(
                Request2("http://tuna.be/show/thumb/" + param, method="HEAD")
            )
        except urllib2.HTTPError as e:
            if e.code == 404:
                raise PictureNotFoundError()
            else:
                raise e
            
        url = response.geturl()

        try:
            response = urllib2.build_opener(DontRedirectHandler).open(
                Request2(url.replace(".jpg", "_org.jpg"), method="HEAD")
            )
            org = response.geturl()
        except urllib2.HTTPError as e:
            if e.code == 404:
                org = "http://tuna.be/show/thumb/" + param
            else:
                raise e
            
        self.insert_all(cursor, table, (param, org))
        return org
    
    def get_full(self, match):
        return self.work(match)

    def get_full_https(self, match):
        return None
    
    def get_large(self, match):
        return "http://tuna.be/show/thumb/" + self.get_parameters(match)

    def get_large_https(self, match):
        return None
    
    def get_thumb(self, match):
        return self.get_large(match)

    def get_thumb_https(self, match):
        return None

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
