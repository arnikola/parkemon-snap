package com.daprlabs.aaron.parkemonsnapapp

import retrofit2.Call
import retrofit2.Retrofit
import retrofit2.converter.moshi.MoshiConverterFactory
import retrofit2.http.GET
import retrofit2.http.Query

/**
 * Created by Default on 30/07/2016.
 */

class WebAPI {


    private val api : Api

    init {
        val retrofit = Retrofit.Builder()
                .baseUrl("api url")
                .addConverterFactory(MoshiConverterFactory.create()).build()

        api = retrofit.create(Api::class.java)
    }

}

interface  Api {
    @GET("/top.json")
    fun getTop(@Query("after") after: String,
               @Query("limit") limit: String)
            : Call<Park>;
}

class Park (
        val name : String,
        val location : String
)


