# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
import urlparse
from private_constant import *

class pixiv:
	def __str__(self):
		return "pixiv"
	
	regexStr = "^http://(?:www\\.)?pixiv\\.net/(?:index|member_illust)\\.php\\?(?:.*)&?illust_id=(\\d+)(?:&.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	imageRegex = re.compile("<div class=\"front-centered\"><a href=\"login.php\\?illust_id=\\d+\" style=\"display:block;line-height:0;\"><img src=\"(http://[a-z0-9/_\\-\\.]+)_m(\\.\\w+)\" alt=\".+\" title=\".+\" border=\"0\" /></a></div>", re.IGNORECASE)
	
	last = None
	
	def getUriData(self, match):
		id = match.group(1)
		
		if self.last is not None and str(self.last[0]) == id:
			return self.last
		
		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()
		
		c.execute("SELECT * FROM pixiv WHERE id = " + db.literal(id))
		sqlResult = c.fetchone()
		
		if sqlResult is None:
			httpRes = urllib2.urlopen("http://www.pixiv.net/member_illust.php?mode=medium&illust_id=" + id)
			html = httpRes.read().decode("utf-8")
			
			if html.find("<span class=\"error\">") != -1:
				return None
			
			match = self.imageRegex.search(html)
			prefix = match.group(1)
			extension = match.group(2)
			
			c.execute("INSERT INTO pixiv VALUES (%s, %s, %s)"
				% (db.literal(id), db.literal(prefix), db.literal(extension))
			)
			
			db.commit()
			
			self.last = [id, prefix, extension]
		else:
			self.last = sqlResult
		
		return self.last
	
	def getFullSize(self, match):
		data = self.getUriData(match)
		
		return (data[1] + data[2]) if data is not None else None
	
	def getLargeSize(self, match):
		data = self.getUriData(match)
		
		return (data[1] + "_m" + data[2]) if data is not None else None
	
	def getThumbnail(self, match):
		data = self.getUriData(match)
		
		return (data[1] + "_s" + data[2]) if data is not None else None