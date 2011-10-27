namespace org.dicomcs.data
{
	using System;
	using org.dicomcs.data;
	using org.dicomcs.dict;
	
	/// <summary>
	/// Filtered Dataset
	/// Used by DICOMDIR
	/// </summary>
	public abstract class FilterDataset : BaseDataset
	{
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'AnonymousClassIterator' to access its enclosing instance. 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="jlca1019"'
		public class AnonymousClassIterator : Iterator
		{
			public AnonymousClassIterator(FilterDataset enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(FilterDataset enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
				next = findNext();
			}
			private FilterDataset enclosingInstance;
			public FilterDataset Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			//UPGRADE_NOTE: The initialization of  'next' was moved to method 'InitBlock'. 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="jlca1005"'
			private DcmElement next;
			
			private DcmElement findNext()
			{
				DcmElement el;
				while (backendIter.hasNext())
				{
					if (filter((el = (DcmElement) backendIter.next()).tag()))
					{
						return el;
					}
				}
				return null;
			}
			
			public virtual bool hasNext()
			{
				return next != null;
			}
			
			public virtual System.Object next()
			{
				if (next == null)
				{
					throw new System.Exception();
				}
				DcmElement retval = next;
				next = findNext();
				return retval;
			}
			
			public virtual void  remove()
			{
				throw new UnsupportedOperationException();
			}
		}
		public virtual Charset Charset
		{
			get
			{
				return backend.Charset;
			}
			
		}
		public virtual Dataset Parent
		{
			get
			{
				return backend.Parent;
			}
			
		}
		public virtual System.String PrivateCreatorID
		{
			get
			{
				return backend.PrivateCreatorID;
			}
			
			set
			{
				backend.PrivateCreatorID = value;
			}
			
		}
		public virtual long ItemOffset
		{
			get
			{
				return backend.ItemOffset;
			}
			
			set
			{
				throw new UnsupportedOperationException();
			}
			
		}
		public virtual DcmHandler DcmHandler
		{
			get
			{
				throw new UnsupportedOperationException();
			}
			
		}
		public virtual DefaultHandler SAXHandler
		{
			get
			{
				throw new UnsupportedOperationException();
			}			
		}
		
		protected internal BaseDataset backend;
		
		/// <summary>
		/// Creates a new instance of DatasetView 
		/// </summary>
		public FilterDataset(Dataset backend)
		{
			this.backend = (BaseDataset) backend;
		}
		
		protected internal abstract bool filter(uint tag);
		
		public override Iterator iterator()
		{
			Iterator backendIter = backend.GetEnumerator();
			return new AnonymousClassIterator(this);
		}
		
		public virtual bool isEmpty()
		{
			return size() == 0;
		}
		
		
		protected internal virtual DcmElement put(DcmElement el)
		{
			if (!filter(el.tag()))
			{
				throw new System.ArgumentException("" + el + " does not fit in this sub DataSet");
			}
			return backend.put(el);
		}
		
		public override DcmElement remove(uint tag)
		{
			return filter(tag)?backend.remove(tag):null;
		}
		
		public override void  clear()
		{
			ArrayList toRemove = new ArrayList();
			for (Iterator iter = backend.iterator(); iter.hasNext(); )
			{
				DcmElement el = (DcmElement) iter.next();
				if (filter(el.tag()))
				{
					toRemove.add(el);
				}
			}
			 for (int i = 0, n = toRemove.size(); i < n; ++i)
			{
				backend.remove(((DcmElement) toRemove.get(i)).tag());
			}
		}
		
		
		public override void  readDataset(System.IO.Stream in_Renamed, DcmDecodeParam param, int stopTag)
		{
			throw new UnsupportedOperationException();
		}
		
		public override void  readFile(System.IO.Stream in_Renamed, FileFormat format, int stopTag)
		{
			throw new UnsupportedOperationException();
		}
				
		public sealed class Selection : FilterDataset
		{
			private Dataset filterDs;
			
			internal Selection(Dataset backend, Dataset filter):base(backend)
			{
				this.filterDs = filter;
			}
			
			public int size()
			{
				if (filterDs == null)
				{
					return Enclosing_Instance.backend.size();
				}
				int count = 0;
				 for (Iterator iter = iterator(); iter.hasNext(); )
				{
					iter.next();
					++count;
				}
				return count;
			}
			
			protected internal bool filter(int tag)
			{
				return filterDs == null || filter.contains(tag);
			}
			
			public bool contains(int tag)
			{
				return filter(tag) && Enclosing_Instance.backend.contains(tag);
			}
			
			public DcmElement get(int tag)
			{
				if (filterDs == null)
				{
					return Enclosing_Instance.backend.get(tag);
				}
				DcmElement filterEl = filter.get(tag);
				if (filterEl == null)
				{
					return null;
				}
				
				DcmElement el = Enclosing_Instance.backend.get(tag);
				if (!(el is SQElement))
				{
					return el;
				}
				if (!(filterEl is SQElement))
				{
					log.warn("VR mismatch - dataset:" + el + ", filter:" + filterEl);
					return el;
				}
				if (filterEl.isEmpty())
				{
					return el;
				}
				return new FilterSQElement((SQElement) el, filterEl.Item);
			}
		}
		
		sealed class Segment : FilterDataset
		{
			private uint fromTag;
			private uint toTag;
			
			internal Segment(Dataset backend, uint fromTag, uint toTag):base(backend)
			{
				this.fromTag = fromTag & 0xFFFFFFFFL;
				this.toTag = toTag & 0xFFFFFFFFL;
				if (this.fromTag > this.toTag)
				{
					throw new System.ArgumentException("fromTag:" + Tags.toString(fromTag) + " greater toTag:" + Tags.toString(toTag));
				}
			}
			
			public int size()
			{
				int count = 0;
				long ltag;
				 for (Iterator iter = Enclosing_Instance.backend.iterator(); iter.hasNext(); )
				{
					ltag = ((DcmElement) iter.next()).tag() & (int) (- (0x100000000 - 0xFFFFFFFFL));
					if (ltag < fromTag)
						continue;
					if (ltag >= toTag)
						break;
					++count;
				}
				return count;
			}
			
			protected internal bool filter(uint tag)
			{
				uint ltag = tag & 0xFFFFFFFF;
				return ltag >= fromTag && ltag < toTag;
			}
			
			public DcmElement get(uint tag)
			{
				return filter(tag)?Enclosing_Instance.backend.get(tag):null;
			}
		}
	}
}