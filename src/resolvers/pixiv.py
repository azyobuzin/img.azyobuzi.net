# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
import urlparse
from sgmllib import SGMLParser
from private_constant import *

class pixiv:
    def __str__(self):
        return "pixiv"

    regexStr = "^http://(?:www\\.)?pixiv\\.net/(?:index|member_illust)\\.php\\?(?:.*)&?illust_id=(\\d+)(?:&.*)?$"
    regex = re.compile(regexStr, re.IGNORECASE)

    def getFullSize(self, match):
        return "http://img.azyobuzi.net/api/pixiv_proxy.cgi?size=full&id=" + match.group(1)

    def getLargeSize(self, match):
        return "http://img.azyobuzi.net/api/pixiv_proxy.cgi?size=large&id=" + match.group(1)

    def getThumbnail(self, match):
        return "http://img.azyobuzi.net/api/pixiv_proxy.cgi?size=thumb&id=" + match.group(1)
