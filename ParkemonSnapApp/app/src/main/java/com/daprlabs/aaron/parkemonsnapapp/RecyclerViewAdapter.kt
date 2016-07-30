package com.daprlabs.aaron.parkemonsnapapp

import android.support.v7.widget.RecyclerView
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView

/**
 * Created by Default on 30/07/2016.
 */

class RecyclerViewAdapter
(private val mDataset: Array<String>) : RecyclerView.Adapter<RecyclerViewAdapter.ViewHolder>() {


    // Provide a reference to the views for each data item
    // Complex data items may need more than one view per item, and
    // you provide access to all the views for a data item in a view holder
    class ViewHolder(// each data item is just a string in this case
            var mTextView: TextView) : RecyclerView.ViewHolder(mTextView)


    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder? {
        return null
    }

    // Replace the contents of a view (invoked by the layout manager)
    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        // - get element from your dataset at this position
        // - replace the contents of the view with that element
        holder.mTextView.text = mDataset[position]
    }

    // Return the size of your dataset (invoked by the layout manager)
    override fun getItemCount(): Int {
        return mDataset.size
    }

}
