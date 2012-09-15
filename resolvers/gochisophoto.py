# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
from private_constant import *

class gochisophoto:
	def __str__(self):
		return u"Google ごちそうフォト"
	
	regexStr = "^http://(?:www\\.)?gochisophoto\\.com/photo/([\\w\\-]+)/(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	detailRegex = re.compile("<div class=\"detailPhoto\"><span><img src=\"(https://[a-z0-9/_\\-\\.]+/)s960(/[\\w\\-%]+)\" alt=\"\" width=\"\\d+\" /></span></div>", re.IGNORECASE)
	
	last = None
	
	def getUriData(self, match):
		id = match.group(1)
		
		if self.last is not None and self.last[0] == id:
			return self.last
		
		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()
		
		c.execute("SELECT * FROM gochisophoto WHERE id = " + db.literal(id))
		
		sqlResult = c.fetchone()
		
		if sqlResult is None:
			httpRes = None
			
			try:
				httpRes = urllib2.urlopen("http://www.gochisophoto.com/photo/%s/" % id)
			except:
				return None
			
			html = httpRes.read().decode("utf-8")
			match = self.detailRegex.search(html)
			
			prefix = match.group(1)
			postfix = match.group(2)
			
			c.execute("INSERT INTO gochisophoto VALUES (%s, %s, %s)"
				% (db.literal(id), db.literal(prefix), db.literal(postfix))
			)
			
			db.commit()
			
			self.last = [id, prefix, postfix]
		else:
			self.last = sqlResult
		
		return self.last
	
	def getFullSize(self, match):
		data = self.getUriData(match)
		return ("%ss960%s" % (data[1], data[2])) if data is not None else None
	
	def getLargeSize(self, match):
		data = self.getUriData(match)
		return ("%ss960%s" % (data[1], data[2])) if data is not None else None
	
	def getThumbnail(self, match):
		data = self.getUriData(match)
		return ("%ss240%s" % (data[1], data[2])) if data is not None else None
