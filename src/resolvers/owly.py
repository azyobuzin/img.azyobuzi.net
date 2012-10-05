# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
from private_constant import *

class owly:
	def __str__(self):
		return "Ow.ly"
	
	regexStr = "^http://(?:www\\.)?ow\\.ly/i/(\\w+)(?:/(?:original/?)?)?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	imageRegex = re.compile("<div class=\"imageWrapper\">\\s*<img src=\"(http://[a-z0-9/_\\-\\.]+)\" alt=\".*\" title=\".*\" />\\s*</div>", re.IGNORECASE)
	
	last = None
	
	def getUriData(self, match):
		id = match.group(1)
		
		if self.last is not None and self.last[0] == id:
			return self.last
		
		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()
		
 		c.execute("SELECT * FROM owly WHERE id = " + db.literal(id))
		sqlResult = c.fetchone()
		
		if sqlResult is None:
			reqUri = "http://ow.ly/i/%s/original" % id
			httpRes = None
			
			try:
				httpRes = urllib2.urlopen(reqUri)
			except:
				return None
			
			html = httpRes.read().decode("utf-8")
			match = self.imageRegex.search(html)
			
			original = match.group(1)
			
			c.execute("INSERT INTO owly VALUES (%s, %s)"
				% (db.literal(id), db.literal(original))
			)
			
			db.commit()
			
			self.last = [id, original]
		else:
			self.last = sqlResult
		
		return self.last
	
	def getFullSize(self, match):
		data = self.getUriData(match)
		return data[1] if data is not None else None
	
	def getLargeSize(self, match):
		return "http://static.ow.ly/photos/normal/%s.jpg" % match.group(1)
	
	def getThumbnail(self, match):
		return "http://static.ow.ly/photos/thumb/%s.jpg" % match.group(1)
